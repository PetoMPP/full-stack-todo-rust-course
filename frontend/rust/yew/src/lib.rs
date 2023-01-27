use api::{auth::auth::Auth, tasks::todo_task::TodoTask};
use serde::{Serialize, Deserialize};
use yew::prelude::*;
use yew_router::prelude::*;
use stylist::{yew::{styled_component, Global}, style};
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
    tasks: Option<Vec<TodoTask>>,
    tasks_valid: bool,
}

#[styled_component(App)]
pub fn app() -> Html {
    let body_style = style!(
        r#"
        -ms-overflow-style: none;  /* Internet Explorer 10+ */
        scrollbar-width: none;  /* Firefox */
        overflow-y: auto;

        ::-webkit-scrollbar { 
            display: none;  /* Safari and Chrome */
        }
        "#).unwrap();
    html! {
        <>
        <Global css={MAIN_STYLESHEET}/>
        <BrowserRouter>
            <Navbar />
            <div class={body_style}>
                <Switch<Route> render={Switch::render(switch)}/>
            </div>
        </BrowserRouter>
        </>
    }
}