<script lang="ts">
  import svelteLogo from './assets/svelte.svg'
  import viteLogo from '/vite.svg'
  import Counter from './lib/Counter.svelte'
  import TodoList from './lib/TodoList.svelte'
  import UserProfile from './lib/UserProfile.svelte'
  import RegistrationForm from './lib/RegistrationForm.svelte'
  import LoginForm from './lib/LoginForm.svelte'
  import {onMount} from "svelte";
  
  let weatherResults:any = []
  let currentTab = 'home'
  
  onMount(() => {
    fetch('/api/weather')
      .then(response => response.json())
      .then(data => {
        weatherResults = data
      })
  })
  
  function setTab(tab: string) {
    currentTab = tab
  }
</script>

<main>
  <header class="app-header">
    <div class="logo-container">
      <a href="https://vitejs.dev" target="_blank" rel="noreferrer">
        <img src={viteLogo} class="logo" alt="Vite Logo" />
      </a>
      <a href="https://svelte.dev" target="_blank" rel="noreferrer">
        <img src={svelteLogo} class="logo svelte" alt="Svelte Logo" />
      </a>
    </div>
    <h1>FluentUIScaffold Sample App</h1>
  </header>

  <nav class="app-nav">
    <button 
      class="nav-button" 
      class:active={currentTab === 'home'}
      on:click={() => setTab('home')}
      data-testid="nav-home"
    >
      Home
    </button>
    <button 
      class="nav-button" 
      class:active={currentTab === 'todos'}
      on:click={() => setTab('todos')}
      data-testid="nav-todos"
    >
      Todos
    </button>
    <button 
      class="nav-button" 
      class:active={currentTab === 'profile'}
      on:click={() => setTab('profile')}
      data-testid="nav-profile"
    >
      Profile
    </button>
    <button 
      class="nav-button" 
      class:active={currentTab === 'register'}
      on:click={() => setTab('register')}
      data-testid="nav-register"
    >
      Register
    </button>
    <button 
      class="nav-button" 
      class:active={currentTab === 'login'}
      on:click={() => setTab('login')}
      data-testid="nav-login"
    >
      Login
    </button>
  </nav>

  <div class="app-content">
    {#if currentTab === 'home'}
      <section class="home-section">
        <h2>Welcome to <span class="brand">FluentUIScaffold Sample App</span></h2>
        <p class="subtitle">This is a sample application demonstrating various UI interactions for testing with the FluentUIScaffold E2E testing framework.</p>
        
        <div class="card">
          <h3>Counter Component</h3>
          <Counter />
        </div>

        <div class="weather-section">
          <h3>Weather Data</h3>
          {#each weatherResults as weather}
            <div class="weather-card" data-testid="weather-item">
              <h4>{weather.date}</h4>
              <p>Temperature: <span class="weather-temp">{weather.temperatureC}°C</span></p>
              <p>Summary: <span class="weather-summary">{weather.summary}</span></p>
            </div>
          {/each}
        </div>
      </section>
    {:else if currentTab === 'todos'}
      <section class="todos-section">
        <TodoList />
      </section>
    {:else if currentTab === 'profile'}
      <section class="profile-section">
        <UserProfile />
      </section>
    {:else if currentTab === 'register'}
      <section class="register-section">
        <RegistrationForm />
      </section>
    {:else if currentTab === 'login'}
      <section class="login-section">
        <LoginForm />
      </section>
    {/if}
  </div>

  <footer class="app-footer">
    <p class="read-the-docs">
      Click on the Vite and Svelte logos to learn more
    </p>
  </footer>
</main>

<style>
  :global(body) {
    background: #f4f6fa;
    color: #222;
    font-family: 'Segoe UI', 'Roboto', 'Arial', sans-serif;
    margin: 0;
    min-height: 100vh;
  }
  main {
    min-height: 100vh;
    background: #f4f6fa;
    padding-bottom: 2rem;
  }
  .app-header {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    padding: 1.5rem 2rem 1rem 2rem;
    background: #23272f;
    color: #fff;
    border-bottom: 2px solid #007bff;
    box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    position: relative;
    z-index: 2;
  }
  .logo-container {
    display: flex;
    gap: 0.5rem;
  }
  .logo {
    height: 2.5em;
    padding: 0.25em;
    will-change: filter;
    transition: filter 300ms;
    background: #fff;
    border-radius: 0.5em;
    box-shadow: 0 1px 4px rgba(0,0,0,0.08);
  }
  .logo:hover {
    filter: drop-shadow(0 0 2em #646cffaa);
  }
  .logo.svelte:hover {
    filter: drop-shadow(0 0 2em #ff3e00aa);
  }
  .app-header h1 {
    font-size: 2.2rem;
    font-weight: 700;
    letter-spacing: 0.01em;
    margin: 0;
    opacity: 0.98;
    text-shadow: 0 2px 8px rgba(0,0,0,0.08);
  }
  .app-nav {
    display: flex;
    gap: 1rem;
    padding: 1rem 2rem;
    background: #fff;
    border-bottom: 1px solid #e9ecef;
    box-shadow: 0 1px 4px rgba(0,0,0,0.03);
    position: sticky;
    top: 0;
    z-index: 1;
  }
  .nav-button {
    padding: 0.5rem 1.5rem;
    border: none;
    background: #e9ecef;
    color: #23272f;
    border-radius: 0.375rem;
    font-size: 1rem;
    font-weight: 500;
    cursor: pointer;
    transition: background 0.2s, color 0.2s;
    outline: none;
    box-shadow: 0 1px 2px rgba(0,0,0,0.03);
  }
  .nav-button.active, .nav-button:active {
    background: #007bff;
    color: #fff;
    font-weight: 600;
    box-shadow: 0 2px 8px rgba(0,123,255,0.08);
  }
  .nav-button:hover:not(.active) {
    background: #d0e3ff;
    color: #007bff;
  }
  .app-content {
    padding: 2.5rem 1rem 1rem 1rem;
    max-width: 900px;
    margin: 0 auto;
  }
  .home-section, .todos-section, .profile-section, .register-section, .login-section {
    max-width: 700px;
    margin: 0 auto;
  }
  .home-section h2 {
    font-size: 1.6rem;
    font-weight: 700;
    margin-bottom: 0.5rem;
    text-align: center;
    color: #23272f;
  }
  .brand {
    color: #007bff;
    font-weight: 800;
    letter-spacing: 0.01em;
  }
  .subtitle {
    text-align: center;
    color: #555;
    margin-bottom: 2rem;
    font-size: 1.1rem;
  }
  .card {
    padding: 1.5rem;
    border-radius: 0.75rem;
    background: #fff;
    box-shadow: 0 2px 12px rgba(0,0,0,0.07);
    margin-bottom: 2rem;
    text-align: center;
  }
  .card h3 {
    margin-top: 0;
    margin-bottom: 1rem;
    font-size: 1.2rem;
    color: #007bff;
    font-weight: 600;
  }
  .weather-section {
    margin-top: 2rem;
  }
  .weather-section h3 {
    color: #23272f;
    font-size: 1.1rem;
    font-weight: 700;
    margin-bottom: 1rem;
    text-align: left;
  }
  .weather-card {
    padding: 1.25rem 1.5rem;
    border-radius: 0.5rem;
    background: #f8fafc;
    box-shadow: 0 1px 6px rgba(0,0,0,0.06);
    margin-bottom: 1.2rem;
    color: #23272f;
    font-size: 1rem;
    font-weight: 500;
    text-align: left;
    border: 1px solid #e3e8ee;
  }
  .weather-card h4 {
    margin: 0 0 0.5rem 0;
    font-size: 1.05rem;
    color: #007bff;
    font-weight: 600;
    word-break: break-all;
  }
  .weather-temp {
    color: #e67e22;
    font-weight: 600;
  }
  .weather-summary {
    color: #007bff;
    font-weight: 500;
  }
  .app-footer {
    padding: 1.5rem 0 0.5rem 0;
    text-align: center;
    background: transparent;
    color: #888;
    font-size: 1rem;
    border-top: none;
  }
  .read-the-docs {
    color: #888;
    margin: 0;
    font-size: 0.95rem;
  }
  @media (max-width: 700px) {
    .app-header, .app-nav {
      flex-direction: column;
      align-items: flex-start;
      padding: 1rem;
    }
    .app-header h1 {
      font-size: 1.3rem;
    }
    .app-content {
      padding: 1rem 0.5rem;
    }
    .home-section, .todos-section, .profile-section, .register-section, .login-section {
      padding: 0 0.5rem;
    }
    .card, .weather-card {
      padding: 1rem;
    }
  }
</style>
