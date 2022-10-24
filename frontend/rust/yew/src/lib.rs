use api::{auth::auth::Auth, tasks::task::Task};
use serde::{Serialize, Deserialize};
use yew::prelude::*;
use yew_router::prelude::*;
use stylist::yew::{styled_component, Global};
use yewdux::prelude::*;

use crate::router::{Route, switch};

mod components;
use components::organisms::navbar::Navbar;
mod router;
mod styles;
mod api;
mod utils;

const MAIN_STYLESHEET: &str = include_str!("main.css");

#[derive(Default, PartialEq, Clone, Debug, Store, Serialize, Deserialize)]
#[store(storage="session", storage_tab_sync)]
pub struct SessionStore{
    user: Option<Auth>
}

#[derive(Default, PartialEq, Clone, Debug, Store)]
pub struct TaskStore{
    tasks: Option<Vec<Task>>,
    tasks_valid: bool,
}

#[styled_component(App)]
pub fn app() -> Html {
    html! {
        <>
        <Global css={MAIN_STYLESHEET}/>
        <BrowserRouter>
            <Navbar />
            <div style={"overflow-y: auto;"}>
                <Switch<Route> render={Switch::render(switch)}/>
            </div>
        </BrowserRouter>
        </>
    }
}