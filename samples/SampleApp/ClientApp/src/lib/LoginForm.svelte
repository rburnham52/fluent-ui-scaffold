<script lang="ts">
  let email = '';
  let password = '';
  let welcomeMessage = '';
  let errorMessage = '';
  let showWelcome = false;
  let showError = false;

  function handleSubmit() {
    // Reset messages
    showWelcome = false;
    showError = false;
    welcomeMessage = '';
    errorMessage = '';

    // Simple validation
    if (!email || !password) {
      errorMessage = 'Email and password are required';
      showError = true;
      return;
    }

    if (!email.includes('@')) {
      errorMessage = 'valid email address';
      showError = true;
      return;
    }

    // Simulate login: accept any valid email with the test password
    if (password === 'password123') {
      welcomeMessage = 'Login successful!';
      showWelcome = true;
      
      // Clear form
      email = '';
      password = '';
    } else {
      errorMessage = 'Invalid credentials';
      showError = true;
    }
  }
</script>

<div class="login-form">
  <h2>User Login</h2>
  <form id="loginForm" novalidate on:submit|preventDefault={handleSubmit}>
    <div class="form-group">
      <label for="email-input">Email:</label>
      <input 
        type="email" 
        id="email-input" 
        name="email" 
        bind:value={email}
      />
    </div>
    <div class="form-group">
      <label for="password-input">Password:</label>
      <input 
        type="password" 
        id="password-input" 
        name="password" 
        bind:value={password}
      />
    </div>
    <div class="form-group">
      <button type="submit" id="login-button">Login</button>
    </div>
  </form>
  
  {#if showWelcome}
    <div id="success-message" class="message success">
      {welcomeMessage}
    </div>
  {/if}
  
  {#if showError}
    <div id="error-message" class="message error">
      {errorMessage}
    </div>
  {/if}
</div>

<style>
  .login-form {
    max-width: 400px;
    margin: 50px auto;
    padding: 20px;
    border: 1px solid #ccc;
    border-radius: 5px;
    background: #fff;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  }

  .form-group {
    margin-bottom: 15px;
  }

  .form-group label {
    display: block;
    margin-bottom: 5px;
    font-weight: bold;
    color: #333;
  }

  .form-group input {
    width: 100%;
    padding: 8px;
    border: 1px solid #ccc;
    border-radius: 3px;
    font-size: 14px;
  }

  .form-group input:focus {
    outline: none;
    border-color: #28a745;
    box-shadow: 0 0 0 2px rgba(40,167,69,0.25);
  }

  .form-group button {
    width: 100%;
    padding: 10px;
    background-color: #28a745;
    color: white;
    border: none;
    border-radius: 3px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
  }

  .form-group button:hover {
    background-color: #218838;
  }

  .message {
    margin-top: 15px;
    padding: 10px;
    border-radius: 3px;
  }

  .success {
    background-color: #d4edda;
    color: #155724;
    border: 1px solid #c3e6cb;
  }

  .error {
    background-color: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
  }

  h2 {
    text-align: center;
    color: #333;
    margin-bottom: 20px;
  }
</style> 