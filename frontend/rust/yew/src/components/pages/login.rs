use crate::{
    api::auth::auth_service::AuthService,
    components::{
        atoms::{button::Button, text_input::TextInput},
        organisms::error_message::{ErrorMessage, DEFAULT_TIMEOUT_MS},
    },
    router::Route,
    styles::{color::Color, styles::Styles},
    SessionStore, TaskStore,
};
use gloo::timers::callback::Timeout;
use lazy_static::__Deref;
use uuid::Uuid;
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::*;
use yewdux::prelude::*;

use super::{auth_data::AuthData, error_data::ErrorData};

#[function_component(Login)]
pub fn create_account() -> Html {
    let auth_data = use_mut_ref(|| AuthData::default());

    let onchange = {
        let auth_data = auth_data.clone();
        Callback::from(move |event: Event| {
            let auth_data = auth_data.clone();
            let target_element = event.target_unchecked_into::<HtmlInputElement>();
            match target_element.id().as_str() {
                "username" => auth_data.borrow_mut().username = target_element.value(),
                "password" => auth_data.borrow_mut().password = target_element.value(),
                _ => (),
            };
        })
    };

    let error_data = use_state(|| ErrorData::default());

    let (_, session_dispatch) = use_store::<SessionStore>();
    let (_, task_dispatch) = use_store::<TaskStore>();
    let history = use_history().unwrap();

    let onsubmit = {
        let error_data = error_data.clone();
        Callback::from(move |event: FocusEvent| {
            event.prevent_default();
            let auth_data = auth_data.clone();
            let session_dispatch = session_dispatch.clone();
            let task_dispatch = task_dispatch.clone();
            let history = history.clone();
            let error_data = error_data.clone();
            spawn_local(async move {
                let response = AuthService::login(
                    auth_data.borrow().username.clone(),
                    auth_data.borrow().password.clone(),
                )
                .await;
                match response {
                    Ok(auth) => {
                        session_dispatch.clone().reduce(|store| {
                            let mut store = store.deref().clone();
                            store.user = Some(auth);
                            store
                        });
                        task_dispatch.reduce(|_| {
                            TaskStore::default()
                        });
                        history.push(Route::Home)
                    }
                    Err(error) => {
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
                    }
                }
            });
        })
    };

    let style = Styles::get_form_style();

    html! {
        <>
        if error_data.display {
            <ErrorMessage message={error_data.message.clone()}/>
        }
        <form class={style} {onsubmit}>
            <h2 class={Color::Secondary.into_style("color")}>{"Login"}</h2>
            <TextInput id={"username"} onchange={onchange.clone()} label={"Your username"} placeholder={"enter username.."} data_test={"username"}/>
            <TextInput id={"password"} {onchange} label={"Your password"} input_type={"password"} placeholder={"enter password.."} data_test={"password"}/>
            <Button label={"Log in!"} data_test={"submit"}/>
        </form>
        </>
    }
}
