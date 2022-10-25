use reqwasm::http::{Headers, Method};

use crate::api::{api_client::{ApiClient, ApiError}, api_error_response::ApiErrorResponse};

use super::{task::Task, tasks_response::{TasksResponse, TaskResponse}};

pub struct TasksService;

const TASKS_URI: &str = "/tasks";

impl TasksService {
    pub async fn create_task(token: String, task: Task) -> Result<Task, ApiError> {
        let response: Result<Result<TaskResponse, ApiErrorResponse>, ApiError> = ApiClient::send_json(
            TASKS_URI,
            Method::POST,
            Some(serde_json::to_string(&task).unwrap()),
            Some(TasksService::get_headers(token)),
        )
        .await;

        return match response {
            Ok(ok) => match ok {
                Ok(ok) => Ok(ok.data),
                Err(error) => Err(ApiError::Other(error.error)),
            },
            Err(error) => Err(error),
        };
    }

    pub async fn update_task(token: String, task: Task) -> Result<(), ApiError> {
        let response = ApiClient::send_text(
            format!("{}/{}", TASKS_URI, &task.id).as_str(),
            Method::PATCH,
            Some(serde_json::to_string(&task).unwrap()),
            Some(TasksService::get_headers(token)),
        )
        .await;

        return match response {
            Ok(_) => Ok(()),
            Err(error) => Err(error),
        };
    }

    pub async fn delete_task(token: String, id: i32) -> Result<(), ApiError> {
        let body: Option<&str> = None;
        let response = ApiClient::send_text(
            format!("{}/{}", TASKS_URI, id).as_str(),
            Method::DELETE,
            body,
            Some(TasksService::get_headers(token)),
        )
        .await;

        return match response {
            Ok(_) => Ok(()),
            Err(error) => Err(error),
        };
    }

    pub async fn get_tasks(token: String) -> Result<Vec<Task>, ApiError> {
        let body: Option<&str> = None;
        let response: Result<Result<TasksResponse, ApiErrorResponse>, ApiError> = ApiClient::send_json(
            TASKS_URI,
            Method::GET,
            body,
            Some(TasksService::get_headers(token)),
        )
        .await;

        return match response {
            Ok(ok) => match ok {
                Ok(ok) => Ok(ok.data),
                Err(error) => Err(ApiError::Other(error.error)),
            },
            Err(error) => Err(error),
        };
    }

    fn get_headers(token: String) -> Headers {
        let headers = Headers::default();
        headers.append("content-type", "application/json");
        headers.append("x-auth-token", &token);
        headers
    }
}
