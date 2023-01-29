use serde::*;

use super::api_client::ApiError;

#[derive(Serialize, Deserialize, Debug)]
pub struct ApiErrorResponse {
    pub error: String
}

impl Into<ApiError> for ApiErrorResponse {
    fn into(self) -> ApiError {
        ApiError::Other(self.error)
    }
}