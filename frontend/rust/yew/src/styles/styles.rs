use stylist::{Style, style};

use super::color::Color;

pub struct Styles;

impl Styles {
    pub fn get_editable_details_style() -> (Style, Style) {
        let style = style!(
            r#"
            padding: 10px;
            margin: auto;
            display: flex;
            flex-direction: column;
            width: 80vw;

            div {
                margin: 0.5em 0;
            }
            "#
        )
        .unwrap();
    
        let button_style = style!(
            r#"
            margin-top: 2vw;
            display: flex;
            justify-content: space-between;
            flex-wrap: wrap;
            "#
        )
        .unwrap();

        (style, button_style)

    }

    pub fn get_table_style() -> (Style, Style) {
        let style = Style::new(format!(
            r#"
            margin-top: 1em;

            th {{
                color: {secondary};
                background-color: {info};
                font-weight: bold;
                border-bottom: 0.75em solid;
                border-color: {secondary};
            }}
    
            table {{
                width: 100%;
                margin: auto;
                background-color: {info};
            }}
    
            tr {{
                color: black;
                background-color: {primary};
                border-bottom: 0.25em solid;
                border-top: 0.25em solid;
                border-color: {secondary};
            }}
    
            th, td {{
                padding: 0.5em;
                text-align: center;
                vertical-align: middle;
            }}

            div {{
                justify-content: center;
            }}
            "#,
            info = Color::Info.get_css_color(),
            primary = Color::Primary.get_css_color(),
            secondary = Color::Secondary.get_css_color(),
        ))
        .unwrap();

        let div_style = style!(
            r#"
            display: flex;
            justify-content: space-between;
            margin: auto;
            flex-flow: wrap-reverse;
            "#).unwrap();

        (style, div_style)
    }

    pub fn get_link_style(fore_color: Option<Color>, back_color: Option<Color>, hover_color: Option<Color>) -> Style {
        let fore_color = match fore_color.clone() {
            Some(color) => color.clone().get_css_color(),
            None => Color::Primary.get_css_color()
        };
    
        let hover_color = match hover_color.clone() {
            Some(color) => color.clone().get_css_color(),
            None => fore_color.clone()
        };
    
        let mut style_string = format!(
            r#"
            margin: 1em;
            text-decoration: none;
            color: {};
            :hover {{
                color: {};
                text-decoration: underline;
            }}
            "#,
            fore_color, hover_color);
    
        if let Some(color) = back_color.clone() {
            style_string = format!("{}background-color: {};", style_string, color.clone().get_css_color());
        }

        Style::new(style_string).unwrap()
    }

    pub fn get_home_style() -> Style {
        style!(
            r#"
            padding: 1em;
            margin: auto;
            display: flex;
            flex-direction: column;
            width: 80vw;
            div {
                margin: 1em 0;
            }
            "#
        )
        .unwrap()
    }

    pub fn get_form_style() -> Style {
        style!(
            r#"
            padding: 10px;
            margin: auto;
            display: flex;
            flex-direction: column;
            width: min(80vw, 850px);
            div {
                padding: 10px;
            }
            button {
                margin: auto;
            }
            "#
        )
        .unwrap()
    }

    pub fn get_navbar_styles(fore_color: Option<Color>, back_color: Option<Color>) -> (Style, Style) {
        
    let fore_color = match fore_color.clone() {
        Some(color) => color.get_css_color(),
        None => Color::Primary.get_css_color(),
    };

    let mut style_string = format!(
        r#"
        border-bottom: 1px solid {fore_color};
        color: {fore_color};
        display: flex;
        flex-direction: row;
        justify-content: space-between;
    "#,
        fore_color = fore_color
    );

    if let Some(back_color) = back_color.clone() {
        style_string = format!("{} background-color: {}", style_string, back_color.get_css_color());
    }

    let style = Style::new(style_string).unwrap();

    let div_style_string = format!(
        r#"
            padding: 2vw;
        "#
    );

    let div_style = Style::new(div_style_string).unwrap();

    (style, div_style)
    }
}