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
        if let Some(user) = store.user.clone() {
            <div>
                <h4 data-test={"welcome"}>{format!("Welcome, {name}!", name = user.username)}</h4>
                <p>{"Here you can add, delete and modify your tasks!"}</p>
            </div>
            <Tasks error_data={Some(error_data)} />
        }
        else {
            <div>
                <p>{"Here you could add, delete and modify your tasks, if you were logged in.."}</p>
            </div>
        }
        </div>
        </>
    }
}
