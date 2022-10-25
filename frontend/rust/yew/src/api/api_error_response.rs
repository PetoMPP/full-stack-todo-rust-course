use serde::*;

#[derive(Serialize, Deserialize, Debug)]
pub struct ApiErrorResponse {
    pub error: String
}