use stylist::yew::styled_component;
use yew::prelude::*;

use crate::{api::tasks::todo_task::TodoTask, router::Route, components::atoms::{checkbox::Checkbox, route_link::RouteLink}, styles::color::Color};

#[derive(Properties, PartialEq)]
pub struct TaskProperties {
    pub todo_task: TodoTask,
    pub remove_onclick: Callback<MouseEvent>,
    pub toggle_completed: Callback<MouseEvent>
}

#[styled_component(Task)]
pub fn task(props: &TaskProperties) -> Html {
    let task = props.todo_task.clone();
    let remove_onclick = props.remove_onclick.clone();
    let toggle_completed = props.toggle_completed.clone();
    html! {
        <tr>
            <td data-test={"priority"}>
                {
                    match &task.priority {
                    Some(p) => p.to_string(),
                    None => "-".to_string()
                    }
                }
            </td>
            <td>
                <Checkbox data_test={"completed"} checked={task.completed()} onclick={toggle_completed}/>
            </td>
            <td>
                <RouteLink data_test={"tasklink"} link={Route::TaskDetails { id: task.id }} text={task.title.clone()} fore_color={Color::CustomStr("black".to_string())} />
            </td>
            <td>
                <RouteLink data_test={"delete"} link={Route::Home} onclick={remove_onclick} text={"❌"} fore_color={Color::Error} />
            </td>
        </tr>
    }
}