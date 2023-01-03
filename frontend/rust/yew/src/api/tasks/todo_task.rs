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
    pub created_at: String,
    pub completed_at: Option<String>,
    pub user_id: i32
}

impl TodoTask {
    pub fn completed(&self) -> bool {
        self.completed_at.is_some()
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
