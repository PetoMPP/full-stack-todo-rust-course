use crate::{
    api::tasks::{task::Task, tasks_service::TasksService},
    components::{
        atoms::{
            button::Button,
            checkbox::Checkbox,
            dropdown::Dropdown,
            text_input::{ControlType, TextInput},
        },
        organisms::error_message::ErrorMessage,
        pages::{
            error_data::ErrorData,
            task_details::{get_priority_options, get_selected_value},
        },
    },
    router::Route,
    styles::{color::Color, styles::Styles},
    SessionStore, TaskStore, utils::handle_api_error,
};
use chrono::Local;
use lazy_static::__Deref;
use stylist::yew::styled_component;
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::*;
use yewdux::prelude::*;

#[styled_component(NewTask)]
pub fn new_task() -> Html {
    let (style, button_style) = Styles::get_editable_details_style();

    let history = use_history().unwrap();
    let history = history.clone();
    let goto_home = {
        let history = history.clone();
        Callback::from(move |_| history.push(Route::Home))
    };

    let error_data = use_state(|| ErrorData::default());

    let (session_store, session_dispatch) = use_store::<SessionStore>();
    let (_, task_dispatch) = use_store::<TaskStore>();

    let task_data = use_mut_ref(|| Task::default());

    let task_dispatch = task_dispatch.clone();
    let onchange = {
        let task_data = task_data.clone();
        Callback::from(move |event: Event| {
            let target_element = event.target_unchecked_into::<HtmlInputElement>();
            let value = target_element.value();
            match target_element.id().as_str() {
                "title" => task_data.borrow_mut().title = value.clone(),
                "priority" => {
                    task_data.borrow_mut().priority = match value.parse() {
                        Ok(priority) => Some(priority),
                        Err(_) => None,
                    }
                }
                "description" => {
                    task_data.borrow_mut().description = if value == "" {
                        None
                    } else {
                        Some(value.clone())
                    }
                }
                "completed" => {
                    task_data.borrow_mut().completed_at = if task_data.borrow_mut().completed() {
                        None
                    } else {
                        Some(Local::now().to_string())
                    }
                }
                _ => (),
            };
        })
    };

    let create_task = {
        let error_data = error_data.clone();
        let history = history.clone();
        let token = match session_store.user.clone() {
            Some(user) => Some(user.token.clone()),
            None => None,
        };
        let task_data = task_data.clone();

        let task_dispatch = task_dispatch.clone();
        let session_dispatch = session_dispatch.clone();
        Callback::from(move |_: MouseEvent| {
            let history = history.clone();
            let task_dispatch = task_dispatch.clone();
            let session_dispatch = session_dispatch.clone();
            let token = token.clone();
            let task: Task = task_data.deref().clone().into();
            let error_data = error_data.clone();

            if let None = token {
                return;
            }

            spawn_local(async move {
                let response =
                    TasksService::create_task(token.clone().unwrap(), task.clone()).await;
                match response {
                    Ok(_) => {
                        history.push(Route::Home);
                        task_dispatch.reduce(|store| {
                            let mut store = store.deref().clone();
                            store.tasks_valid = false;
                            store
                        })
                    }
                    Err(error) => handle_api_error(error, session_dispatch, Some(error_data))
                }
            })
        })
    };

    html! {
        <>
        if error_data.display {
            <ErrorMessage message={error_data.message.clone()}/>
        }
        <div class={style}>
            <h3>{"Create new task!"}</h3>
            <TextInput data_test={"title"} id={"title"} label={"Title"} onchange={onchange.clone()}/>
            <Dropdown data_test={"priority"} id={"priority"} label={"Priority"} options={get_priority_options()} selected_option={get_selected_value(None)} onchange={onchange.clone()}/>
            <TextInput data_test={"description"} id={"description"} label={"Description"} control_type={ControlType::Textarea} rows={3} onchange={onchange.clone()}/>
            <Checkbox data_test={"completed"} id={"completed"} label={"Completed?"} checked={task_data.borrow().completed()} onchange={onchange.clone()}/>
            <div class={button_style}>
                <Button
                    label={"Cancel"}
                    fore_color={Color::CustomStr("white".to_string())}
                    back_color={Color::Error}
                    hover_color={Color::Error2}
                    data_test={"cancel"}
                    onclick={goto_home.clone()}/>
                <Button label={"Create task"} onclick={create_task.clone()} data_test={"submit"}/>
            </div>
        </div>
        </>
    }
}
