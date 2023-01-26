use stylist::{style, yew::styled_component, Style};
use yew::prelude::*;

use crate::{
    api::tasks::todo_task::TodoTask,
    components::atoms::{checkbox::Checkbox, route_link::RouteLink, priority::Priority},
    router::Route,
    styles::color::Color,
};

#[derive(Properties, PartialEq)]
pub struct TaskProperties {
    pub todo_task: TodoTask,
    pub remove_onclick: Callback<MouseEvent>,
    pub toggle_completed: Callback<MouseEvent>,
}

#[styled_component(Task)]
pub fn task(props: &TaskProperties) -> Html {
    let task = props.todo_task.clone();
    let mut hide_completion = false;
    let (creation_time, creation_date) = match task.created_at() {
        Some(datetime) => (datetime.time().format("%H:%M").to_string(), datetime.date().format("%d/%m/%y").to_string()),
        None => ("Not yet".to_string(), "created!?".to_string())
    };
    let (completion_time, completion_date) = match task.completed_at() {
        Some(datetime) => (datetime.time().format("%H:%M").to_string(), datetime.date().format("%d/%m/%y").to_string()),
        None => {
            hide_completion = true;
            ("Not yet".to_string(), "completed..".to_string())
        } 
    };

    let task_style = Style::new(format!(
        r#"
        margin: 1rem;
        display: flex;
        flex-flow: column;
        width: 40%;
        max-width: 450px;
        border-radius: 20px;
        box-sizing: border-box;
        box-shadow: 8px 8px 5px {highlight2};
        @media only screen and (max-width: 850px) {{
            width: 100%;
        }}
        "#,
        highlight2 = Color::Highlight2.get_css_color()
    ))
    .unwrap();
    let up_style = Style::new(format!(
        r#"
        padding : 0.5rem;
        padding-bottom: 0;
        display: flex;
        border: solid {secondary} 2px;
        border-top-left-radius: 20px;
        border-top-right-radius: 20px;
        background-color: {secondary};
        >div {{
            margin: 0.2rem;
            display: flex;
            align-items: center;
            height: calc(max(2vh, 1em, 1rem) * 2.7);
            max-height: calc(max(2vh, 1em, 1rem) * 2.7);
        }}
        a {{
            font-weight: 500;
        }}
        "#,
        secondary = Color::Secondary.get_css_color()
    ))
    .unwrap();
    let down_style = Style::new(format!(
        r#"
        padding : 0.5rem;
        padding-top: 0;
        display: flex;
        height: calc(max(1.5vh, 0.75em, 0.75rem) * 6 - 2px);
        border: solid {secondary} 2px;
        border-bottom-left-radius: 20px;
        border-bottom-right-radius: 20px;
        background-color: {secondaryBg};
        font-family: monospace;
        p {{
            overflow: hidden;
            color: {secondary};
            padding: 1px;
        }}
        >div {{
            display: flex;
            flex-flow: column;
            justify-content: space-between;
            margin: 0.25rem 0;
        }}
        "#,
        secondary = Color::Secondary.get_css_color(),
        secondaryBg = Color::SecondaryBg.get_css_color()
    ))
    .unwrap();

    let title_style = style!(
        r#"
        width: -webkit-fill-available;
        width: -moz-available;
        "#
    )
    .unwrap();
    
    let remove_style = style!(
        r#"
        align-self: baseline;
        a {
            display: flex;
            align-self: flex-start;
        }
        "#
    )
    .unwrap();

    let date_style = style!(
        r#"
        display: flex;
        width: max-content;
        div {
            display: flex;
            width: max-content;
            flex-flow: column;
            margin-top: auto;
            margin-bottom: auto;
            justify-content: flex-start;
        }
        img {
            display: flex;
            align-self: center;
            width: calc(max(1.5vh, 0.75em, 0.75rem) * 2);
            height: calc(max(1.5vh, 0.75em, 0.75rem) * 2);
            padding-right: 0.25rem;
        }
        "#
    )
    .unwrap();
    let remove_onclick = props.remove_onclick.clone();
    let toggle_completed = props.toggle_completed.clone();
    let priority = match &task.priority {
        Some(p) => p.to_string(),
        None => "-".to_string()
    };
    html! {
        <div class={task_style}>
            <div class={up_style}>
                <div>
                    <Checkbox data_test={"completed"} checked={task.completed()} onclick={toggle_completed} size={"calc(max(2vh, 1em, 1rem) * 1.25)"}/>
                </div>
                <Priority data_test={"priority"} text={priority.clone()}/>
                <div class={title_style}>
                    <RouteLink data_test={"tasklink"} link={Route::TaskDetails { id: task.id }} text={task.title.clone()} fore_color={Color::Highlight} />
                </div>
                <div class={remove_style}>
                    <RouteLink data_test={"delete"} link={Route::Home} onclick={remove_onclick} text={"❌"} fore_color={Color::Error} />
                </div>
            </div>
            <div class={down_style}>
            <div>
                <div class={date_style.clone()}>
                    <img src={"img/pen.png"} alt={"created_at"}/>
                    <div>
                        <div>{creation_time}</div>
                        <div>{creation_date}</div>
                    </div>
                </div>
                <div class={date_style.clone()}>
                if !hide_completion {
                    <img src={"img/tick.png"} alt={"completed_at"}/>
                    <div>
                        <div>{completion_time}</div>
                        <div>{completion_date}</div>
                    </div>
                }
                else {
                    <img style={"display: none;"} src={"img/tick.png"} alt={"completed_at"}/>
                    <div style={"display: none;"}>{completion_time}</div>
                    <div style={"display: none;"}>{completion_date}</div>
                }
                </div>
            </div>
                <p style={"margin-left: 0.25rem;"}>{&task.description.unwrap_or("Go to task details!".to_string())}</p>
            </div>
        </div>
    }
}
