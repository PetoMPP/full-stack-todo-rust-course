use crate::{
    api::auth::auth_service::AuthService,
    components::{
        atoms::{button::Button, text_input::TextInput},
        organisms::error_message::{ErrorMessage, DEFAULT_TIMEOUT_MS},
        stores::error_store::ErrorStore,
    },
    router::Route,
    styles::{color::Color, styles::Styles},
    SessionStore,
};
use gloo::{console::log, timers::callback::Timeout};
use lazy_static::__Deref;
use uuid::Uuid;
use wasm_bindgen_futures::spawn_local;
use web_sys::HtmlInputElement;
use yew::prelude::*;
use yew_router::prelude::*;
use yewdux::prelude::*;

use super::auth_data::AuthData;

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
            log!(format!("{:?}", auth_data.borrow()));
        })
    };

    let (error_store, error_dispatch) = use_store::<ErrorStore>();

    let (_, session_dispatch) = use_store::<SessionStore>();
    let history = use_history().unwrap();

    let onsubmit = {
        let error_dispatch = error_dispatch.clone();
        Callback::from(move |event: FocusEvent| {
            event.prevent_default();
            let auth_data = auth_data.clone();
            let session_dispatch = session_dispatch.clone();
            let history = history.clone();
            let error_dispatch = error_dispatch.clone();
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
                        history.push(Route::Home)
                    }
                    Err(error) => {
                        let error_uuid = Uuid::new_v4();
                        {
                            let error_dispatch = error_dispatch.clone();
                            Timeout::new(DEFAULT_TIMEOUT_MS, move || {
                                error_dispatch.clone().reduce(|store| {
                                    if store.uuid == error_uuid {
                                        ErrorStore::new(String::new(), false, error_uuid).into()
                                    } else {
                                        store
                                    }
                                })
                            })
                            .forget();
                        }
                        error_dispatch
                            .clone()
                            .reduce(|_| ErrorStore::new(String::new(), false, error_uuid));

                        error_dispatch
                            .clone()
                            .reduce(|_| ErrorStore::new(error, true, error_uuid));
                    }
                }
            });
        })
    };

    let style = Styles::get_form_style();

    html! {
        <>
        if error_store.display {
            <ErrorMessage message={error_store.message.clone()}/>
        }
        <form class={style} {onsubmit}>
            <h2 class={Color::Info.into_style("color")}>{"Login"}</h2>
            <TextInput id={"username"} onchange={onchange.clone()} label={"Your username"} placeholder={"enter username.."} data_test={"username"}/>
            <TextInput id={"password"} {onchange} label={"Your password"} input_type={"password"} placeholder={"enter password.."} data_test={"password"}/>
            <Button label={"Log in!"} data_test={"submit"}/>
        </form>
        </>
    }
}
