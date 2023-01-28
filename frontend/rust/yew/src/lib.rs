use std::rc::Rc;

use api::{auth::auth::Auth, tasks::todo_task::TodoTask};
use serde::{Serialize, Deserialize};
use yew::prelude::*;
use yew_router::prelude::*;
use stylist::{yew::{styled_component, Global}, style};
use yewdux::prelude::*;

use crate::{router::{Route, switch}, app_context::AppContext, styles::color::Color};

mod components;
use components::organisms::navbar::Navbar;
mod router;
mod styles;
mod api;
mod utils;
mod app_context;

const MAIN_STYLESHEET: &str = include_str!("main.css");

#[derive(Default, PartialEq, Clone, Debug, Store, Serialize, Deserialize)]
#[store(storage="session", storage_tab_sync)]
pub struct SessionStore{
    user: Option<Auth>,
    theme: Option<String>
}

#[derive(Default, PartialEq, Clone, Debug, Store)]
pub struct TaskStore{
    tasks: Option<Vec<TodoTask>>,
    tasks_valid: bool,
}

#[styled_component(App)]
pub fn app() -> Html {
    let (session_store, _) = use_store::<SessionStore>();
    let theme = session_store.theme.clone();
    let ctx = use_memo(|t| {
        let mut ctx = AppContext::default();
        if let Some(theme) = &t {
            ctx.set_theme(theme.as_str())
        };
        ctx
    }, theme);
    let mut css = MAIN_STYLESHEET.to_string();
    css.push_str(format!(
        r#"
            html, body {{
                background-color: {primaryBg};
            }}

            h1, h2, h3, h4, h5, h6 {{
                color: {highlight};
            }}

            label, p, a {{
                color: {primary};
            }}
        "#,
        primary = Color::Primary.get_css_color(&ctx),
        primaryBg = Color::PrimaryBg.get_css_color(&ctx),
        highlight = Color::Highlight.get_css_color(&ctx),
    ).as_str());
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
        <ContextProvider<Rc<AppContext>> context={ctx}>
            <Global css={css}/>
            <BrowserRouter>
                <Navbar />
                <div class={body_style}>
                    <Switch<Route> render={switch}/>
                </div>
            </BrowserRouter>
        </ContextProvider<Rc<AppContext>>>
        </>
    }
}