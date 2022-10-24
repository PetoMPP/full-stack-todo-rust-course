use reqwasm::http::{Method, Headers};
use serde_json::json;

use crate::api::{api_client::{ApiClient, ApiError}, api_error_response::ApiErrorResponse};

use super::{auth_response::AuthResponse, auth::Auth};

pub struct AuthService;

const LOGIN_URI: &str = "/users/login";
const USERS_URI: &str = "/users";

impl AuthService {
    pub async fn login(username: String, password: String) -> Result<Auth, ApiError> {
        let response: Result<Result<AuthResponse, ApiErrorResponse>, ApiError> = ApiClient::send_json(
            LOGIN_URI,
            Method::POST,
        Some(AuthService::get_auth_body(username, password)),
        Some(AuthService::get_headers())).await;

        return match response {
            Ok(ok) => match ok {
                Ok(ok) => Ok(ok.data),
                Err(error) => Err(ApiError::Other(error.error)),
            },
            Err(error) => Err(error),
        }
    }

    pub async fn register(username: String, password: String) -> Result<Auth, ApiError> {
        let response: Result<Result<AuthResponse, ApiErrorResponse>, ApiError> = ApiClient::send_json(
            USERS_URI,
            Method::POST,
            Some(AuthService::get_auth_body(username, password)),
            Some(AuthService::get_headers()),
        )
        .await;

        return match response {
            Ok(ok) => match ok {
                Ok(ok) => Ok(ok.data),
                Err(error) => Err(ApiError::Other(error.error)),
            },
            Err(error) => Err(error),
        }
    }

    fn get_auth_body(username: String, password: String) -> String {
        json! {
            {
                "username": username,
                "password": password
            }
        }
        .to_string()
    }

    fn get_headers() -> Headers {
        let headers = Headers::default();
        headers.append("content-type", "application/json");
        headers
}

}
