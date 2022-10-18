use uuid::Uuid;

#[derive(Default, PartialEq, Clone, Debug)]
pub struct ErrorData {
    pub message: String,
    pub display: bool,
    pub uuid: Uuid
}