use std::rc::Rc;

use lazy_static::__Deref;
use stylist::yew::styled_component;
use yew::prelude::*;
use yewdux::prelude::use_store;

use crate::app_context::AppContext;
use crate::components::atoms::button::Button;
use crate::{SessionStore, TaskStore};
use crate::components::atoms::route_link::RouteLink;
use crate::router::Route;
use crate::styles::color::Color;
use crate::styles::styles::Styles;

#[derive(Properties, PartialEq)]
pub struct NavbarProperties {
    pub data_test: Option<String>,
    pub fore_color: Option<Color>,
    pub back_color: Option<Color>,
}

#[styled_component(Navbar)]
pub fn navbar(props: &NavbarProperties) -> Html {
    let ctx = use_context::<Rc<AppContext>>().unwrap();
    let (_, task_dispatch) = use_store::<TaskStore>();
    let (session_store, session_dispatch) = use_store::<SessionStore>();
    let (style, div_style) = Styles::get_navbar_styles(&ctx, props.fore_color.as_ref(), props.back_color.as_ref());
    
    let switch_theme = session_dispatch.reduce_mut_callback(move | e | {
        let ctx = ctx.clone();
        let themes = ctx.get_themes();
        let get_theme = ctx.get_theme();
        let curr = themes.iter().enumerate().find(|(_, t)| {
            t.name == get_theme.name
        }).unwrap().0;

        let next = if curr == themes.len() - 1 {
            0
        }
        else {
            curr + 1
        };
        
        e.theme = Some(themes[next].clone().name);
        e.clone()
    });

    let logout = {
        let session_dispatch = session_dispatch.clone();
        let task_dispatch = task_dispatch.clone();
        Callback::from(move |_: MouseEvent| {
            task_dispatch.reduce(|_| {
                TaskStore::default().into()
            });
            session_dispatch.reduce(|session_store| {
                let mut session_store = session_store.deref().clone();
                session_store.user = None;
                session_store.into()
            });
        })
    };

    html! {
        <section class={style}>
            <div class={div_style.clone()}>
                <RouteLink
                    text={"My TODO App"}
                    link={Route::Home}
                    data_test={"logo"}
                    fore_color={props.fore_color.clone()}
                    back_color={props.back_color.clone()}/>
            </div>
            <Button label={"change theme"} onclick={switch_theme}/>
            if let None = session_store.user {
                <div class={div_style}>
                    <RouteLink
                        text={"Login"}
                        link={Route::Login}
                        data_test={"login"}
                        fore_color={props.fore_color.clone()}
                        back_color={props.back_color.clone()}/>
                    <RouteLink
                        text={"Create Account"}
                        link={Route::CreateAccount}
                        data_test={"create-account"}
                        fore_color={Color::Highlight}
                        back_color={props.back_color.clone()}
                        hover_color={Color::Highlight2}/>
                </div>
            }
            else {
                <div class={div_style}>
                    <RouteLink
                        text={"Log out"}
                        link={Route::Home}
                        onclick={logout}
                        data_test={"log-out"}
                        fore_color={Color::Error}
                        back_color={props.back_color.clone()}
                        hover_color={Color::Error2}/>
                </div>
            }
        </section>
    }
}
