use gloo::{console::log, timers::callback::Timeout};
use lazy_static::__Deref;
use uuid::Uuid;
use yew::UseStateHandle;
use yewdux::prelude::Dispatch;

use crate::{api::api_client::ApiError, components::{pages::error_data::ErrorData, organisms::error_message::DEFAULT_TIMEOUT_MS}, SessionStore};

pub fn handle_api_error(
    error: ApiError,
    session_dispatch: Dispatch<SessionStore>,
    error_data: Option<UseStateHandle<ErrorData>>)
{
    log!(error.to_string());
    // if let ApiError::HttpStatus(code, _) = error {
    //     if code == 401u16 || code == 403u16 {
    session_dispatch.reduce(|store| {
        let mut store = store.deref().clone();
        store.user = None;
        store
    });
    //     }
    // }
    
    match error_data {
        Some(error_data) => {
            let error_uuid = Uuid::new_v4();
            {
                let error_data = error_data.clone();
                Timeout::new(DEFAULT_TIMEOUT_MS, move || {
                    if error_data.uuid == error_uuid {
                        error_data.set(ErrorData::default());
                    }
                })
                .forget();
            }
            error_data.set(ErrorData::default());

            error_data.set(ErrorData { message: error.to_string(), display: true, uuid: error_uuid });
        },
        None => log!(error.to_string()),
    }
}