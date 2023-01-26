use stylist::{yew::styled_component, Style};
use yew::prelude::*;

use crate::styles::color::Color;

#[derive(PartialEq, Clone, Copy)]
pub enum LabelLocation {
    Left,
    Right
}

impl Default for LabelLocation {
    fn default() -> Self {
        Self::Left
    }
}

#[derive(Properties, PartialEq)]
pub struct CheckboxProperties {
    pub checked: bool,
    pub label: Option<String>,
    pub label_location: Option<LabelLocation>,
    pub onchange: Option<Callback<Event>>,
    pub onclick: Option<Callback<MouseEvent>>,
    pub size: Option<String>, // TODO: use CssSize or whatever
    pub id: Option<String>,
    pub data_test: Option<String>,
}

#[styled_component(Checkbox)]
pub fn checkbox(props: &CheckboxProperties) -> Html {
    let label_style = Style::new(format!(
        r#"
        margin-right: 20px;
        color: {};
    "#,
        Color::Primary.get_css_color()
    ))
    .unwrap();

    let main_style = Style::new(
        r#"
        display: flex;
        flex-direction: row;
        align-items: center;
    "#,
    )
    .unwrap();

    let style = Style::new(format!(
        r#"
        accent-color: {color};
        background-color: {back_color};
        height: {size};
        width: {size};
        "#,
        color = Color::Highlight.get_css_color(),
        back_color = Color::Secondary.get_css_color(),
        size = props.size.clone().unwrap_or("max(2vh, 1em, 1rem)".to_string())
    ))
    .unwrap();

    let label_location = props.label_location.unwrap_or_default();
    html! {
        <div class={main_style}>
        if label_location == LabelLocation::Left {
            if let Some(label) = props.label.clone() {
                <label class={label_style.clone()}>{label}</label>
            }
        }
            <input 
                id={props.id.clone()}
                type={"checkbox"}
                data-test={props.data_test.clone()}
                class={style}
                checked={props.checked.clone()}
                onchange={props.onchange.clone()}
                onclick={props.onclick.clone()}/>
        if label_location == LabelLocation::Right {
            if let Some(label) = props.label.clone() {
                <label class={label_style.clone()}>{label}</label>
            } 
        }
        </div>
    }
}
