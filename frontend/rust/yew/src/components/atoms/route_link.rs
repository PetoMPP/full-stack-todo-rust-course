use std::rc::Rc;

use stylist::yew::styled_component;
use yew::prelude::*;
use yew_router::prelude::*;

use crate::{
    router::Route,
    styles::{color::Color, styles::Styles}, app_context::AppContext,
};

#[derive(Properties, PartialEq)]
pub struct RouteLinkProperties {
    pub text: String,
    pub link: Option<Route>,
    pub onclick: Option<Callback<MouseEvent>>,
    pub data_test: Option<String>,
    pub fore_color: Option<Color>,
    pub back_color: Option<Color>,
    pub hover_color: Option<Color>,
}

#[styled_component(RouteLink)]
pub fn link(props: &RouteLinkProperties) -> Html {
    let ctx: Rc<AppContext> = use_context().unwrap();
    let classes = classes!(Styles::get_link_style(
        &ctx,
        props.fore_color.as_ref(),
        props.back_color.as_ref(),
        props.hover_color.as_ref()
    ));

    html! {
        if let Some(route) = props.link.clone() {
            <a data-test={props.data_test.clone()} onclick={props.onclick.clone()}>
                <Link<Route> to={route} {classes}>{&props.text}</Link<Route>>
            </a>
        }
        else {
            <a class={classes} data-test={props.data_test.clone()} onclick={props.onclick.clone()}>
                {&props.text}
            </a>
        }
    }
}
