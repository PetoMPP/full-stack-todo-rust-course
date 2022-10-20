use std::collections::HashMap;

use stylist::Style;

#[derive(PartialEq, Clone, Eq, Hash)]
pub enum Color {
    Primary,
    Secondary,
    Info,
    Highlight,
    Highlight2,
    Error,
    Error2,
    CustomStr(String)
}

#[derive(PartialEq, Clone, Eq, Hash)]
pub struct CssColor {
    r: u8,
    g: u8,
    b: u8,
    a: u8
}

impl CssColor {
    pub fn new(r: u8, g: u8, b: u8, a: u8) -> Self {
        let a = if a > 100 {100} else {a};
        Self { r, g, b, a}
    }
}

impl Color {
    pub fn into_style(&self, target: &str) -> Style {
        Style::new(format!("{}: {};", target, self.get_css_color()))
        .unwrap()
    }

    pub fn get_css_color(&self) -> String {
        let color_values = Self::get_color_values();
        match self {
            Color::CustomStr(color) => color.to_owned(),
            color => {
                let css_color = color_values.get(color).unwrap();
                format!("rgba({}, {}, {}, {}%)", css_color.r, css_color.g, css_color.b, css_color.a)
            }
        }
    }

    fn get_color_values() -> HashMap<Color, CssColor> {
        HashMap::from([
            (Color::Primary, CssColor::new(142, 202, 230, 100)),
            (Color::Secondary, CssColor::new(2, 48, 71, 100)),
            (Color::Info, CssColor::new(33, 156, 186, 100)),
            (Color::Highlight, CssColor::new(255, 183, 3, 100)), 
            (Color::Highlight2, CssColor::new(251, 133, 0, 100)),
            (Color::Error, CssColor::new(158, 42, 43, 100)),
            (Color::Error2, CssColor::new(213, 47, 49, 100))
         ])
    }
}