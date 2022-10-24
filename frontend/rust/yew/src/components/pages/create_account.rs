use crate::{
    api::auth::auth_service::AuthService,
    components::{
        atoms::{button::Button, text_input::TextInput},
        organisms::error_message::ErrorMessage
    },
    router::Route,
    styles::{color::Color, styles::Styles},
    SessionStore, TaskStore, utils::handle_api_error,
};
use lazy_static::__Deref;
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::*;
use yewdux::prelude::*;

use super::{auth_data::AuthData, error_data::ErrorData};

#[function_component(CreateAccount)]
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
            let error_data = error_data.clone();
            let session_dispatch = session_dispatch.clone();
            let task_dispatch = task_dispatch.clone();
            let history = history.clone();
            spawn_local(async move {
                let response = AuthService::register(
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
                    Err(error) => handle_api_error(error, session_dispatch, Some(error_data))
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
            <h2 class={Color::Secondary.into_style("color")}>{"Create account"}</h2>
            <TextInput id={"username"} onchange={onchange.clone()} label={"Your username"} placeholder={"enter username.."} data_test={"username"}/>
            <TextInput id={"password"} {onchange} label={"Your password"} input_type={"password"} placeholder={"enter password.."} data_test={"password"}/>
            <Button label={"Create account!"} data_test={"submit"}/>
        </form>
        </>
    }
}
