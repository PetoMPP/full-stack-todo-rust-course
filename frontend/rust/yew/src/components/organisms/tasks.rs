use chrono::Local;
use gloo::{console::log, timers::callback::Timeout};
use lazy_static::__Deref;
use uuid::Uuid;
use std::{rc::Rc, cmp::Ordering};
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::{use_history, History};
use yewdux::prelude::*;

use crate::{
    api::tasks::{task::{Task, Priority}, tasks_service::TasksService},
    components::{atoms::{
        button::Button,
        checkbox::Checkbox,
        dropdown::{Dropdown, DropdownOption},
        route_link::RouteLink,
    }, pages::error_data::ErrorData},
    router::Route,
    styles::{color::Color, styles::Styles},
    SessionStore, TaskStore,
};

use super::error_message::DEFAULT_TIMEOUT_MS;

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
    let (session_store, _) = use_store::<SessionStore>();
    let (task_store, task_dispatch) = use_store::<TaskStore>();

    let token = match session_store.user.clone() {
        Some(user) => Some(user.token),
        None => None,
    };

    if let Some(token) = token.clone() {
        let task_store = task_store.clone();
        let task_dispatch = task_dispatch.clone();
        update_tasks_in_store(token, task_store, task_dispatch, props.error_data.clone());
    }

    let mut tasks: Vec<Task> = Vec::new();

    if let Some(new_tasks) = task_store.deref().clone().tasks {
        tasks = new_tasks;
    }

    let filter_state = use_state(|| FilterMode::None);
    let sort_state = use_state(|| SortMode::Created);

    tasks = filter_tasks(tasks, *filter_state);
    tasks = sort_tasks(tasks, *sort_state);

    let token = token.clone();
    let output = tasks.iter().map(|task| {
        let token = token.clone();
        let task = task.clone();
        let task_dispatch = task_dispatch.clone();
        let remove_onclick = delete_task_callback(
            task.clone(), task_dispatch.clone(), token.clone().unwrap(), || {}, props.error_data.clone());

        let toggle_completed = toggle_completed_callback(
            task.clone(), task_dispatch.clone(), token.clone().unwrap(), props.error_data.clone());

        html! {
            <tr>
                <td data-test={"priority"}>
                    {
                        match &task.priority {
                        Some(p) => p.to_string(),
                        None => "-".to_string()
                        }
                    }
                </td>
                <td>
                    <Checkbox data_test={"completed"} checked={task.completed()} onclick={toggle_completed}/>
                </td>
                <td>
                    <RouteLink data_test={"tasklink"} link={Route::TaskDetails { id: task.id }} text={task.title.clone()} fore_color={Color::CustomStr("black".to_string())} />
                </td>
                <td>
                    <RouteLink data_test={"delete"} link={Route::Home} onclick={remove_onclick} text={"❌"} fore_color={Color::Error} />
                </td>
            </tr>
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

    let history = use_history().unwrap();
    let new_task = Callback::from(move |_| {
        let history = history.clone();
        history.push(Route::NewTask);
    });

    let (style, dropdown_style) = Styles::get_table_style();
    
    html! {
        <>
            <div class={dropdown_style}>
                <Dropdown label={"Filter"} options={get_filter_options()} data_test={"filter"} selected_option={get_filter_selected_option()} onchange={apply_filter}/>
                <Dropdown label={"Sort"} options={get_sort_options()} data_test={"sort"} selected_option={get_sort_selected_option()} onchange={apply_sort}/>
            </div>
            <Button label={"+ add new task"} onclick={new_task} data_test={"add-task"}/>
            <div class={style}>
                <table>
                <col style="width:10%" />
                <col style="width:10%" />
                <col style="width:60%" />
                <col style="width:20%" />
                    <thead>
                        <th>{"Priority"}</th>
                        <th>{"Completed?"}</th>
                        <th>{"Title"}</th>
                        <th></th>
                    </thead>
                    {for output}
                </table>
            </div>
        </>
    }
}

fn sort_tasks(mut tasks: Vec<Task>, sort: SortMode) -> Vec<Task> {
    let sort = get_sort(sort);
    tasks.sort_by(sort);
    tasks
}

fn get_sort(sort: SortMode) -> impl FnMut(&Task, &Task) -> Ordering {
    match sort {
        SortMode::Title => |task_a: &Task, task_b: &Task| {
            task_a.title.to_lowercase().cmp(&task_b.title.to_lowercase())
        },
        SortMode::Priority => |task_a: &Task, task_b: &Task| {
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
        SortMode::Created => |task_a: &Task, task_b: &Task| {
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

fn filter_tasks(tasks: Vec<Task>, filter: FilterMode) -> Vec<Task> {
    let mut filter = get_filter(filter);
    tasks.iter().filter_map(|task| filter(task)).collect()
}

fn get_filter(filter: FilterMode) -> impl FnMut(&Task) -> Option<Task> {
    match filter {
        FilterMode::None => move |task: &Task| Some(task.clone()),
        FilterMode::CompletedTasks => move |task: &Task| match task.completed_at {
            Some(_) => Some(task.clone()),
            None => None,
        },
        FilterMode::IncompletedTasks => move |task: &Task| match task.completed_at {
            Some(_) => None,
            None => Some(task.clone()),
        },
        FilterMode::PriorityA => move |task: &Task| match task.priority.clone() {
            Some(priority) => if priority == Priority::A {
                    Some(task.clone())
                }
                else {
                    None
                },
            None => None
        },
        FilterMode::PriorityB => move |task: &Task| match task.priority.clone() {
            Some(priority) => if priority == Priority::B {
                    Some(task.clone())
                }
                else {
                    None
                },
            None => None
        },
        FilterMode::PriorityC => move |task: &Task| match task.priority.clone() {
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
    task: Task,
    session_dispatch: Dispatch<TaskStore>,
    token: String,
    error_data: Option<UseStateHandle<ErrorData>>
) -> Callback<MouseEvent> {
    let mut task = task.clone();
    if let None = task.completed_at {
        task.completed_at = Some(Local::now().to_string());
    } else {
        task.completed_at = None;
    }
    let session_dispatch = session_dispatch.clone();
    let token = token.clone();
    let error_data = error_data.clone();
    Callback::from(move |event: MouseEvent| {
        event.prevent_default(); // lets the form to update checked status
        let token = token.clone();
        let task = task.clone();
        let session_dispatch = session_dispatch.clone();
        let error_data = error_data.clone();
        spawn_local(async move {
            let response = TasksService::update_task(token.clone(), task.clone()).await;
            match response {
                Ok(()) => session_dispatch.reduce(|store| {
                    let mut store = store.deref().clone();
                    store.tasks_valid = false;
                    store
                }),
                Err(error) => match error_data {
                    Some(error_data) => {
                        let error_uuid = Uuid::new_v4();
                        {
                            let error_data = error_data.clone();
                            Timeout::new(DEFAULT_TIMEOUT_MS, move || {
                                if error_data.uuid == error_uuid {
                                    error_data.set(ErrorData::default());
                                }
                            })
                            .forget();
                        }
                        error_data.set(ErrorData::default());

                        error_data.set(ErrorData { message: error, display: true, uuid: error_uuid });
                    },
                    None => log!(format!("task completion switch failed, details: {}", error)),
                }
            }
        })
    })
}

pub fn update_tasks_in_store(
    token: String,
    task_store: Rc<TaskStore>,
    task_dispatch: Dispatch<TaskStore>,
    error_data: Option<UseStateHandle<ErrorData>>
) {
    let task_store = task_store.clone();
    let task_dispatch = task_dispatch.clone();
    let error_data = error_data.clone();
    if !task_store.clone().tasks_valid {
        let task_dispatch = task_dispatch.clone();
        spawn_local(async move {
            let response = TasksService::get_tasks(token).await;
            match response {
                Ok(tasks) => task_dispatch.reduce(|store| {
                    let mut store = store.deref().clone();
                    store.tasks = Some(tasks);
                    store.tasks_valid = true;
                    store
                }),
                Err(error) => match error_data {
                    Some(error_data) => {
                        let error_uuid = Uuid::new_v4();
                        {
                            let error_data = error_data.clone();
                            Timeout::new(DEFAULT_TIMEOUT_MS, move || {
                                if error_data.uuid == error_uuid {
                                    error_data.set(ErrorData::default());
                                }
                            })
                            .forget();
                        }
                        error_data.set(ErrorData::default());

                        error_data.set(ErrorData { message: error, display: true, uuid: error_uuid });
                    },
                    None => log!(format!("fetching tasks failed, details: {}", error)),
                }
            }
        });
    }
}

pub fn delete_task_callback<F>(
    task: Task,
    dispatch: Dispatch<TaskStore>,
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
        let dispatch = dispatch.clone();
        let token = token.clone();
        let action = action.clone();
        let error_data = error_data.clone();
        spawn_local(async move {
            let response = TasksService::delete_task(token.clone(), task_id).await;
            match response {
                Ok(()) => dispatch.reduce(|store| {
                    action();
                    let mut store = store.deref().clone();
                    store.tasks_valid = false;
                    store
                }),
                Err(error) => match error_data {
                    Some(error_data) => {
                        let error_uuid = Uuid::new_v4();
                        {
                            let error_data = error_data.clone();
                            Timeout::new(DEFAULT_TIMEOUT_MS, move || {
                                if error_data.uuid == error_uuid {
                                    error_data.set(ErrorData::default());
                                }
                            })
                            .forget();
                        }
                        error_data.set(ErrorData::default());

                        error_data.set(ErrorData { message: error, display: true, uuid: error_uuid });
                    },
                    None => log!(format!("task deletion failed, details: {}", error)),
                }
            }
        })
    })
}
