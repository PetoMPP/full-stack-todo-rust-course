use serde::{Serialize, Deserialize};

use super::todo_task::TodoTask;

#[derive(Serialize, Deserialize)]
pub struct TasksResponse {
    pub data: Vec<TodoTask>
}

#[derive(Serialize, Deserialize)]
pub struct TaskResponse {
    pub data: TodoTask
}
