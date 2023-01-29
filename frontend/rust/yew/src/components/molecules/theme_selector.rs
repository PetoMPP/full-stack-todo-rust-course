use std::rc::Rc;

use stylist::{style, yew::styled_component, Style};
use yew::prelude::*;
use yewdux::prelude::*;

use crate::{app_context::AppContext, SessionStore, styles::color::Color};

#[derive(Properties, PartialEq)]
pub struct ThemeSelectorProperties {}

#[styled_component(ThemeSelector)]
pub fn theme_selector(_: &ThemeSelectorProperties) -> Html {
    let ctx: Rc<AppContext> = use_context().unwrap();
    let (_, session_dispatch) = use_store::<SessionStore>();
    let themes = ctx.get_themes().clone();

    let show_all = use_state(|| false);
    let toggle_all = {
        let show_all = show_all.clone();
        Callback::from(move |_| show_all.set(!*show_all))
    };
    let hide_all = {
        let show_all = show_all.clone();
        Callback::from(move |_| show_all.set(false))
    };
    let size_mul = match *show_all {
        true => 1.5,
        false => 1.0
    };
    let main_style = style!(
        r#"
        position: absolute;
        top: 3rem;
        display: flex;
        flex-flow: column;
        padding: 2rem 2rem 2rem 0.5rem;
        "#
    )
    .unwrap();
    let thumb_style = Style::new(format!(
        r#"
        display: flex;
        width: calc(max(2vh, 1em, 1rem) * {size_mul});
        height: calc(max(2vh, 1em, 1rem) * {size_mul});
        background: linear-gradient(135deg, {highlight} 40%, {primary_bg} 60%);
        border: solid {primary} 1px;
        border-radius: 4px;
        "#,
        primary = Color::Primary.get_css_color(&ctx),
        primary_bg = Color::PrimaryBg.get_css_color(&ctx),
        highlight = Color::Highlight.get_css_color(&ctx),
    ))
    .unwrap();
    
    let name = &ctx.get_theme().name;
    let thumbs = themes
        .iter()
        .filter_map(|t| {
            if &t.name == name {
                return None;
            }
            let thumb_style = Style::new(format!(
                r#"
                display: flex;
                width: calc(max(2vh, 1em, 1rem) * {size_mul});
                height: calc(max(2vh, 1em, 1rem) * {size_mul});
                background: linear-gradient(135deg, {highlight} 45%, {primary_bg} 55%);
                margin-top: 0.5rem;
                border: solid {primary} 1px;
                border-radius: 4px;
                "#,
                primary = t.get_css_color(&Color::Primary),
                primary_bg = t.get_css_color(&Color::PrimaryBg),
                highlight = t.get_css_color(&Color::Highlight),
            ))
            .unwrap();
            let onclick = switch_theme(t.name.clone(), &session_dispatch);

            Some(html!{
                <div class={thumb_style} {onclick}></div>
            })
        });
    html! {
        <div class={main_style} onmouseleave={hide_all}>
            <div class={thumb_style} onclick={toggle_all}></div>
            if *show_all {
                {for thumbs}
            }

        </div>
    }
}

fn switch_theme(name: String, dispatch: &Dispatch<SessionStore>) -> Callback<MouseEvent> {
    dispatch.reduce_mut_callback(move |store| {
        store.theme = Some(name.clone().to_string());
        store.clone()
    })
}