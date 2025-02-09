import {useState} from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import {useChat} from "ai/react";

function App() {
    const {messages, input, handleSubmit, handleInputChange, isLoading} =
        useChat();
    return (
        <div>
            {messages.map(message => (
                <div key={message.id}>
                    <div>{message.role}</div>
                    <div>{message.content}</div>
                </div>
            ))}

            <form onSubmit={handleSubmit}>
                <input
                    value={input}
                    placeholder="Send a message..."
                    onChange={handleInputChange}
                    disabled={isLoading}
                />
            </form>
        </div>
    )
}

export default App
