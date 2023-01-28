use std::{collections::HashMap, rc::Rc};
use yew::Reducible;

use crate::styles::color::{Color, CssColor};

#[derive(Debug, PartialEq, Clone)]
pub struct AppContext {
    theme: Theme,
    themes_data: Vec<Theme>
}

impl Reducible for AppContext {
    type Action = Theme;

    fn reduce(self: Rc<Self>, action: Self::Action) -> Rc<Self> {
        Self {
            theme: action,
            themes_data: self.themes_data.clone()
        }
        .into()
    }
}

impl Default for AppContext {
    fn default() -> Self {
        let themes_data = Self::get_themes_data();
        Self {
            theme: themes_data[2].clone(),
            themes_data
        }
    }
}

impl AppContext {
    pub fn set_theme(&mut self, name: &str) {
        if let Some(theme) = self.themes_data.iter().find(|t| t.name == name) {
            self.theme = theme.clone();
        }
    }

    pub fn get_theme(&self) -> &Theme {
        &self.theme
    }

    pub fn get_themes(&self) -> &Vec<Theme> {
        &self.themes_data
    }
    
    fn get_themes_data() -> Vec<Theme> {
        vec! [
            Theme {
                name: "Hospital".to_string(),
                color_data: HashMap::from([
                    (Color::Primary, CssColor::new(142, 202, 230, 100)),
                    (Color::PrimaryBg, CssColor::new(2, 48, 71, 100)),
                    (Color::Secondary, CssColor::new(33, 156, 186, 100)),
                    (Color::SecondaryBg, CssColor::new(220, 235, 250, 100)),
                    (Color::Highlight, CssColor::new(255, 183, 3, 100)),
                    (Color::Highlight2, CssColor::new(251, 133, 0, 100)),
                    (Color::Error, CssColor::new(158, 42, 43, 100)),
                    (Color::Error2, CssColor::new(213, 47, 49, 100)),
                ]),
            },
            Theme {
                name: "Neon".to_string(),
                color_data: HashMap::from([
                    (Color::Primary, CssColor::new(225, 176, 219, 100)),
                    (Color::PrimaryBg, CssColor::new(73, 48, 107, 100)),
                    (Color::Secondary, CssColor::new(146, 127, 181, 100)),
                    (Color::SecondaryBg, CssColor::new(225, 205, 181, 100)),
                    (Color::Highlight, CssColor::new(172, 228, 170, 100)), 
                    (Color::Highlight2, CssColor::new(106, 211, 137, 100)),
                    (Color::Error, CssColor::new(158, 42, 43, 100)),
                    (Color::Error2, CssColor::new(213, 47, 49, 100))
                ])
            },
            Theme {
                name: "White Widow".to_string(),
                color_data: HashMap::from([
                    (Color::Primary, CssColor::new(86, 82, 84, 100)),
                    (Color::PrimaryBg, CssColor::new(250, 252, 254, 100)),
                    (Color::Secondary, CssColor::new(182, 185, 185, 100)),
                    (Color::SecondaryBg, CssColor::new(228, 227, 227, 100)),
                    (Color::Highlight, CssColor::new(63, 109, 162, 100)), 
                    (Color::Highlight2, CssColor::new(79, 129, 186, 100)),
                    (Color::Error, CssColor::new(158, 42, 43, 100)),
                    (Color::Error2, CssColor::new(213, 47, 49, 100))
                ])
            }
        ]
    }
}

#[derive(Debug, PartialEq, Clone)]
pub struct Theme {
    pub name: String,
    color_data: HashMap<Color, CssColor>
}

impl Theme {
    pub fn get_css_color(&self, color: &Color) -> String {
        match color {
            Color::CustomStr(color) => color.to_owned(),
            color => {
                let css_color = self.color_data.get(color).unwrap();
                format!("rgba({}, {}, {}, {}%)", css_color.r, css_color.g, css_color.b, css_color.a)
            }
        }
    }
}