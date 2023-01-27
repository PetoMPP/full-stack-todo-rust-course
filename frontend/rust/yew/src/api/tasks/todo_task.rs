use chrono::{Utc, DateTime, TimeZone};
use serde::{Deserialize, Serialize};
use std::{cell::RefCell, fmt::Display, str::FromStr};

#[derive(Serialize, Deserialize, Debug, PartialEq, Clone, PartialOrd)]
pub enum Priority {
    A,
    B,
    C,
}

impl FromStr for Priority {
    type Err = ();

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        match s.to_uppercase().as_str() {
            "A" => Ok(Self::A),
            "B" => Ok(Self::B),
            "C" => Ok(Self::C),
            _ => Err(()),
        }
    }
}

impl Display for Priority {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{:?}", &self)
    }
}

#[derive(Serialize, Deserialize, Debug, PartialEq, Clone, Default)]
pub struct TodoTask {
    pub id: i32,
    pub title: String,
    pub priority: Option<Priority>,
    pub description: Option<String>,
    pub created_at: Option<String>,
    pub completed_at: Option<String>,
    pub user_id: i32
}

const DATE_FORMAT: &str = "%Y-%m-%d %H:%M:%S.%f";

impl TodoTask {
    pub fn completed(&self) -> bool {
        self.completed_at.is_some()
    }

    pub fn created_at(&self) -> Option<DateTime<Utc>> {
        if let None = self.created_at {
            return None;
        }
        return match Utc.datetime_from_str(&self.created_at.clone().unwrap().trim(), DATE_FORMAT) {
            Ok(date) => Some(date),
            Err(_) => None
        };
    }

    pub fn completed_at(&self) -> Option<DateTime<Utc>> {
        if let None = self.completed_at {
            return None;
        }
        return match Utc.datetime_from_str(&self.completed_at.clone().unwrap().trim(), DATE_FORMAT) {
            Ok(date) => Some(date),
            Err(_) => None
        };
    }
}

impl From<RefCell<TodoTask>> for TodoTask {
    fn from(ref_cell: RefCell<TodoTask>) -> Self {
        let ref_cell = ref_cell.borrow();
        Self {
            id: ref_cell.id,
            title: ref_cell.title.clone(),
            priority: ref_cell.priority.clone(),
            description: ref_cell.description.clone(),
            created_at: ref_cell.created_at.clone(),
            completed_at: ref_cell.completed_at.clone(),
            user_id: ref_cell.user_id.clone()
        }
    }
}
