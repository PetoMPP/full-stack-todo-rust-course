use std::borrow::Borrow;

use stylist::{yew::styled_component, Style};
use yew::prelude::*;

use crate::styles::color::Color;

#[derive(Properties, PartialEq)]
pub struct PriorityProperties {
    pub text: String,
    pub data_test: Option<String>,
}

#[styled_component(Priority)]
pub fn priority(props: &PriorityProperties) -> Html {
    let bg_color = match props.text.clone().borrow() {
        "A" => Color::CustomStr("red".to_string()),
        "B" => Color::CustomStr("orange".to_string()),
        "C" => Color::CustomStr("green".to_string()),
        "D" => Color::CustomStr("blue".to_string()),
        _ => Color::CustomStr("gray".to_string())
    };

    let main_style = Style::new(format!(
        r#"
        display: flex;
        background-color: {bg_color};
        color: {fg_color};
        border: solid {fg_color} 2px;
        border-radius: 4px;
        width: max(2vh, 1em, 1rem);
        height: fit-content;
        justify-content: center;
        "#,
        fg_color = "white",
        bg_color = bg_color.get_css_color()
    ))
    .unwrap();

    let data_test = props.data_test.clone().unwrap_or_default();

    html! {
        <div><p class={main_style} data-test={data_test}>{props.text.clone()}</p></div>
    }
}
