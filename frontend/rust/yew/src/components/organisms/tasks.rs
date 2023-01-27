use lazy_static::__Deref;
use std::{rc::Rc, cmp::Ordering};
use stylist::style;
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::{use_history, History};
use yewdux::prelude::*;

use crate::{
    api::tasks::{todo_task::{TodoTask, Priority}, tasks_service::TasksService},
    components::{atoms::{
        button::Button,
        dropdown::{Dropdown, DropdownOption},
    },
    molecules::task::Task,
    pages::error_data::ErrorData},
    router::Route,
    styles::styles::Styles,
    SessionStore, TaskStore, utils::handle_api_error,
};

#[derive(Clone, Copy)]
enum FilterMode {
    None,
    CompletedTasks,
    IncompletedTasks,
    PriorityA,
    PriorityB,
    PriorityC,
}

#[derive(Clone, Copy)]
enum SortMode {
    Title,
    Priority,
    Created
}

#[derive(PartialEq, Properties)]
pub struct TasksProperties {
    pub error_data: Option<UseStateHandle<ErrorData>>
}

#[function_component(Tasks)]
pub fn tasks(props: &TasksProperties) -> Html {
    let (session_store, session_dispatch) = use_store::<SessionStore>();
    let (task_store, task_dispatch) = use_store::<TaskStore>();
    let history = use_history().unwrap();

    let token = match session_store.user.clone() {
        Some(user) => Some(user.token),
        None => None,
    };

    let new_task = Callback::from({
        let history = history.clone();
        move |_| {
            history.push(Route::NewTask);
            }
    });

    if let Some(token) = token.clone() {
        let task_store = task_store.clone();
        let task_dispatch = task_dispatch.clone();
        let session_dispatch = session_dispatch.clone();
        update_tasks_in_store(token, task_store, task_dispatch, session_dispatch, props.error_data.clone());
    }

    let mut tasks: Vec<TodoTask> = Vec::new();

    if let Some(new_tasks) = task_store.deref().clone().tasks {
        tasks = new_tasks;
    }

    let filter_state = use_state(|| FilterMode::None);
    let sort_state = use_state(|| SortMode::Created);

    tasks = filter_tasks(tasks, *filter_state);
    tasks = sort_tasks(tasks, *sort_state);

    let token = token.clone();
    let output = tasks.iter().map(|task|{
        let token = token.clone();
        let task_dispatch = task_dispatch.clone();
        let remove_onclick = delete_task_callback(
            task.clone(), task_dispatch.clone(), session_dispatch.clone(), token.clone().unwrap(), || {}, props.error_data.clone());

        let toggle_completed = toggle_completed_callback(
            task.id, task_dispatch.clone(), session_dispatch.clone(), token.clone().unwrap(), props.error_data.clone());

        let todo_task = task.clone();
        html! {
            <Task {todo_task} {remove_onclick} {toggle_completed}/>
        }
    });
    let filter_state = filter_state.clone();

    let apply_filter = Callback::from(move |event: Event| {
        let target_element = event.target_unchecked_into::<HtmlInputElement>();
        let filter_raw = target_element.value().clone();
        let filter = match filter_raw.as_str() {
            "completed" => FilterMode::CompletedTasks,
            "incompleted" => FilterMode::IncompletedTasks,
            "priority-a" => FilterMode::PriorityA,
            "priority-b" => FilterMode::PriorityB,
            "priority-c" => FilterMode::PriorityC,
            _ => FilterMode::None,
        };
        filter_state.set(filter);
    });

    let sort_state = sort_state.clone();

    let apply_sort = Callback::from(move |event: Event| {
        let target_element = event.target_unchecked_into::<HtmlInputElement>();
        let sort_raw = target_element.value().clone();
        let sort = match sort_raw.as_str() {
            "title" => SortMode::Title,
            "priority" => SortMode::Priority,
            "created" => SortMode::Created,
            _ => SortMode::Created
        };
        sort_state.set(sort);
    });

    let (style, dropdown_style) = Styles::get_table_style();
    let tasks_style = style!(
        r#"
        display: flex;
        justify-content: center;
        flex-wrap: wrap;
        "#)
        .unwrap();

    html! {
        <>
            <div class={dropdown_style}>
                <Dropdown label={"Filter"} options={get_filter_options()} data_test={"filter"} selected_option={get_filter_selected_option()} onchange={apply_filter}/>
                <Dropdown label={"Sort"} options={get_sort_options()} data_test={"sort"} selected_option={get_sort_selected_option()} onchange={apply_sort}/>
                <Button label={"+ add new task"} onclick={new_task} data_test={"add-task"}/>
            </div>
            <div class={tasks_style}>
                {for output}
            </div>
        </>
    }
}

fn sort_tasks(mut tasks: Vec<TodoTask>, sort: SortMode) -> Vec<TodoTask> {
    let sort = get_sort(sort);
    tasks.sort_by(sort);
    tasks
}

fn get_sort(sort: SortMode) -> impl FnMut(&TodoTask, &TodoTask) -> Ordering {
    match sort {
        SortMode::Title => |task_a: &TodoTask, task_b: &TodoTask| {
            task_a.title.to_lowercase().cmp(&task_b.title.to_lowercase())
        },
        SortMode::Priority => |task_a: &TodoTask, task_b: &TodoTask| {
            let a_is_none = task_a.priority.is_none();
            let b_is_none = task_b.priority.is_none();
            
            if a_is_none && b_is_none {
                return Ordering::Equal;
            }

            if a_is_none {
                return Ordering::Greater;
            }

            if b_is_none {
                return Ordering::Less;
            }
            
            task_a.priority.partial_cmp(&task_b.priority).unwrap()
        },
        SortMode::Created => |task_a: &TodoTask, task_b: &TodoTask| {
            task_a.id.cmp(&task_b.id)
        },
    }
}

fn get_sort_selected_option() -> DropdownOption {
    DropdownOption {
        label: Some("Creation time".to_string()),
        value: "created".to_string(),
    }
}

fn get_sort_options() -> Vec<DropdownOption> {
    vec![
        DropdownOption {
            label: Some("Title".to_string()),
            value: "title".to_string(),
        },
        DropdownOption {
            label: Some("Priority".to_string()),
            value: "priority".to_string(),
        },
        DropdownOption {
            label: Some("Creation time".to_string()),
            value: "created".to_string(),
        }
    ]
}

fn filter_tasks(tasks: Vec<TodoTask>, filter: FilterMode) -> Vec<TodoTask> {
    let mut filter = get_filter(filter);
    tasks.iter().filter_map(|task| filter(task)).collect()
}

fn get_filter(filter: FilterMode) -> impl FnMut(&TodoTask) -> Option<TodoTask> {
    match filter {
        FilterMode::None => move |task: &TodoTask| Some(task.clone()),
        FilterMode::CompletedTasks => move |task: &TodoTask| match task.completed_at {
            Some(_) => Some(task.clone()),
            None => None,
        },
        FilterMode::IncompletedTasks => move |task: &TodoTask| match task.completed_at {
            Some(_) => None,
            None => Some(task.clone()),
        },
        FilterMode::PriorityA => move |task: &TodoTask| match task.priority.clone() {
            Some(priority) => if priority == Priority::A {
                    Some(task.clone())
                }
                else {
                    None
                },
            None => None
        },
        FilterMode::PriorityB => move |task: &TodoTask| match task.priority.clone() {
            Some(priority) => if priority == Priority::B {
                    Some(task.clone())
                }
                else {
                    None
                },
            None => None
        },
        FilterMode::PriorityC => move |task: &TodoTask| match task.priority.clone() {
            Some(priority) => if priority == Priority::C {
                    Some(task.clone())
                }
                else {
                    None
                },
            None => None
        },
    }
}

fn get_filter_selected_option() -> DropdownOption {
    DropdownOption {
        label: Some("None".to_string()),
        value: "none".to_string(),
    }
}

fn get_filter_options() -> Vec<DropdownOption> {
    vec![
        DropdownOption {
            label: Some("None".to_string()),
            value: "none".to_string(),
        },
        DropdownOption {
            label: Some("Completed tasks".to_string()),
            value: "completed".to_string(),
        },
        DropdownOption {
            label: Some("Incompleted tasks".to_string()),
            value: "incompleted".to_string(),
        },
        DropdownOption {
            label: Some("Priority A".to_string()),
            value: "priority-a".to_string(),
        },
        DropdownOption {
            label: Some("Priority B".to_string()),
            value: "priority-b".to_string(),
        },
        DropdownOption {
            label: Some("Priority C".to_string()),
            value: "priority-c".to_string(),
        },
    ]
}

fn toggle_completed_callback(
    task_id: i32,
    tasks_dispatch: Dispatch<TaskStore>,
    session_dispatch: Dispatch<SessionStore>,
    token: String,
    error_data: Option<UseStateHandle<ErrorData>>
) -> Callback<MouseEvent> {
    let tasks_dispatch = tasks_dispatch.clone();
    let session_dispatch = session_dispatch.clone();
    let token = token.clone();
    let error_data = error_data.clone();
    Callback::from(move |event: MouseEvent| {
        event.prevent_default(); // lets the form to update checked status
        let token = token.clone();
        let tasks_dispatch = tasks_dispatch.clone();
        let session_dispatch = session_dispatch.clone();
        let error_data = error_data.clone();
        spawn_local(async move {
            let response = TasksService::task_toggle_completed(token.clone(), task_id).await;
            match response {
                Ok(()) => tasks_dispatch.reduce(|store| {
                    let mut store = store.deref().clone();
                    store.tasks_valid = false;
                    store
                }),
                Err(error) => handle_api_error(error, &session_dispatch, error_data)
            }
        })
    })
}

pub fn update_tasks_in_store(
    token: String,
    task_store: Rc<TaskStore>,
    task_dispatch: Dispatch<TaskStore>,
    session_dispatch: Dispatch<SessionStore>,
    error_data: Option<UseStateHandle<ErrorData>>
) {
    let task_store = task_store.clone();
    let task_dispatch = task_dispatch.clone();
    let session_dispatch = session_dispatch.clone();
    let error_data = error_data.clone();
    if !task_store.clone().tasks_valid {
        let task_dispatch = task_dispatch.clone();
        let session_dispatch = session_dispatch.clone();
        return spawn_local(async move {
            let response = TasksService::get_tasks(token).await;
            match response {
                Ok(tasks) => task_dispatch.reduce(|store| {
                    let mut store = store.deref().clone();
                    store.tasks = Some(tasks);
                    store.tasks_valid = true;
                    store
                    }
                ),
                Err(error) => handle_api_error(error, &session_dispatch, error_data)
            }
        });
    }
}

pub fn delete_task_callback<F>(
    task: TodoTask,
    tasks_dispatch: Dispatch<TaskStore>,
    session_dispatch: Dispatch<SessionStore>,
    token: String,
    action: F,
    error_data: Option<UseStateHandle<ErrorData>>
) -> Callback<MouseEvent>
where
    F: Fn() + Clone + 'static,
{
    let token = token.clone();
    let action = action.clone();
    let error_data = error_data.clone();
    Callback::from(move |_: MouseEvent| {
        let task_id = task.id.clone();
        let tasks_dispatch = tasks_dispatch.clone();
        let session_dispatch = session_dispatch.clone();
        let token = token.clone();
        let action = action.clone();
        let error_data = error_data.clone();
        spawn_local(async move {
            let response = TasksService::delete_task(token.clone(), task_id).await;
            match response {
                Ok(()) => tasks_dispatch.reduce(|store| {
                    action();
                    let mut store = store.deref().clone();
                    store.tasks_valid = false;
                    store
                }),
                Err(error) => handle_api_error(error, &session_dispatch, error_data)
            }
        })
    })
}
