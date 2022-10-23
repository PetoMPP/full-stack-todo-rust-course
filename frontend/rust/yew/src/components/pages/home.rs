use yew::prelude::*;
use yewdux::prelude::use_store;

use crate::{
    components::organisms::{error_message::ErrorMessage, tasks::Tasks},
    styles::styles::Styles,
    SessionStore,
};

use super::error_data::ErrorData;

#[function_component(Home)]
pub fn home() -> Html {
    let error_data = use_state(|| ErrorData::default());
    let (store, _) = use_store::<SessionStore>();

    let style = Styles::get_home_style();

    html! {
        <>
        if error_data.display {
            <ErrorMessage message={error_data.message.clone()}/>
        }
        <div class={style}>
        <h2>{"Your TODO list"}</h2>
        if let Some(user) = store.user.clone() {
            <p data-test={"welcome"}>{format!("Welcome, {name}!", name = user.username)}</p>
            <p>{"Here you can add, delete and modify your tasks!"}</p>
            <Tasks error_data={Some(error_data)} />
        }
        else {
            <p>{"Here you could add, delete and modify your tasks, if you were logged in.."}</p>
        }
        </div>
        </>
    }
}
