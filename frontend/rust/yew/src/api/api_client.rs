use std::fmt::Display;

use serde::{Deserialize, Serialize, de::DeserializeOwned};
use reqwasm::{http::*, Error};
use lazy_static::lazy_static;
use wasm_bindgen::JsValue;

const API_CONFIG_RAW_JSON: &str = include_str!("api_settings.json");

lazy_static! {
    static ref API_CONFIG: ApiConfig = serde_json::from_str(API_CONFIG_RAW_JSON).unwrap();
}

#[derive(Serialize, Deserialize)]
struct ApiConfig {
    api_uri: String
}

pub enum ApiError {
    HttpStatus(u16, String),
    Parse(String),
    Other(String)
}

impl Display for ApiError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.to_string())
    }
}

impl ApiError {
    pub fn to_string(&self) -> String {
        match self {
            ApiError::HttpStatus(code, message) => format!("{}: {}", code, message),
            ApiError::Parse(error) => error.to_owned(),
            ApiError::Other(error) => error.to_owned(),
        }
    }
}

pub struct ApiClient;

impl ApiClient {
    pub async fn send_text(uri: &str, method: Method, body: Option<impl Into<JsValue>>, headers: Option<impl Into<Headers>>) -> Result<String, ApiError> {
        let mut request = Request::new(&format!("{}{}", API_CONFIG.api_uri, uri)).method(method);
            
        if let Some(body) = body {
            request = request.body(body);
        }

        if let Some(headers) = headers {
            request = request.headers(headers.into());
        }

        let response = request.send().await;

        return match Self::ensure_success_status_code(&response) {
            Some(error) => Err(error),
            None => Ok(response.unwrap().text().await.unwrap())
        };
    }

    pub async fn send_json<T, E>(uri: &str, method: Method, body: Option<impl Into<JsValue>>, headers: Option<impl Into<Headers>>) -> Result<T, ApiError>
    where 
        T: DeserializeOwned,
        E: Into<ApiError> + DeserializeOwned {
        let mut request = Request::new(&format!("{}{}", API_CONFIG.api_uri, uri)).method(method);
            
        if let Some(body) = body {
            request = request.body(body);
        }

        if let Some(headers) = headers {
            request = request.headers(headers.into());
        }

        let response = request.send().await;

        let error = Self::ensure_success_status_code(&response);

        return match error {
            Some(error) => match response {
                Ok(response) => Self::parse_response::<T, E>(response).await,
                Err(_) => Err(error)
            },
            None => Self::parse_response::<T, E>(response.unwrap()).await,
        };
    }

    fn ensure_success_status_code(response: &Result<Response, Error>) -> Option<ApiError> {
        match response {
            Ok(response) => 
                match response.status() {
                    200..=299 => None,
                    _ => Some(ApiError::HttpStatus(response.status(), response.status_text()))
                }
            Err(error) => Some(ApiError::Other(error.to_string())),
        }
    }

    async fn parse_response<T, E>(response: Response) -> Result<T, ApiError>
    where 
        T: DeserializeOwned,
        E: Into<ApiError> + DeserializeOwned {
            let response_clone = Response::from_raw(response.as_raw().clone().unwrap());
            return match response_clone.json::<T>().await {
                Ok(ok) => Ok(ok),
                Err(_) => {
                    let response_clone = Response::from_raw(response.as_raw().clone().unwrap());
                    match response_clone.json::<E>().await {
                        Ok(ok) => Err(ok.into()),
                        Err(error) => Err(ApiError::Parse(error.to_string())),
                }}
            };
    }
}