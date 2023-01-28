use serde::{Deserialize, Serialize};
use stylist::Style;

use crate::AppContext;

#[derive(PartialEq, Clone, Eq, Hash, Serialize, Deserialize, Debug)]
pub enum Color {
    Primary,
    PrimaryBg,
    Secondary,
    SecondaryBg,
    Highlight,
    Highlight2,
    Error,
    Error2,
    CustomStr(String)
}

#[derive(PartialEq, Clone, Eq, Hash, Serialize, Deserialize, Debug)]
pub struct CssColor {
    pub r: u8,
    pub g: u8,
    pub b: u8,
    pub a: u8
}

impl CssColor {
    pub fn new(r: u8, g: u8, b: u8, a: u8) -> Self {
        let a = if a > 100 {100} else {a};
        Self { r, g, b, a}
    }
}

impl Color {
    pub fn into_style(&self, target: &str, ctx: &AppContext) -> Style {
        Style::new(format!("{}: {};", target, self.get_css_color(ctx)))
        .unwrap()
    }

    pub fn get_css_color(&self, ctx: &AppContext) -> String {
        ctx.get_theme().get_css_color(&self)
    }
}