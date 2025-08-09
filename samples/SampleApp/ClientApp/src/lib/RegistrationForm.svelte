<script lang="ts">
  let email = '';
  let password = '';
  let firstName = '';
  let lastName = '';
  let successMessage = '';
  let errorMessage = '';
  let showSuccess = false;
  let showError = false;

  function handleSubmit() {
    // Reset messages
    showSuccess = false;
    showError = false;
    successMessage = '';
    errorMessage = '';

    // Simple validation
    if (!email || !password || !firstName || !lastName) {
      errorMessage = 'All fields are required';
      showError = true;
      return;
    }

    if (password.length < 8) {
      errorMessage = 'at least 8 characters';
      showError = true;
      return;
    }

    if (!email.includes('@')) {
      errorMessage = 'valid email address';
      showError = true;
      return;
    }

    // Simulate successful registration
    successMessage = 'Registration successful!';
    showSuccess = true;
    
    // Clear form
    email = '';
    password = '';
    firstName = '';
    lastName = '';
  }
</script>

<div class="registration-form">
  <h2>User Registration</h2>
  <form id="registrationForm" novalidate on:submit|preventDefault={handleSubmit}>
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
      <label for="first-name-input">First Name:</label>
      <input 
        type="text" 
        id="first-name-input" 
        name="firstName" 
        bind:value={firstName}
      />
    </div>
    <div class="form-group">
      <label for="last-name-input">Last Name:</label>
      <input 
        type="text" 
        id="last-name-input" 
        name="lastName" 
        bind:value={lastName}
      />
    </div>
    <div class="form-group">
      <button type="submit" id="register-button">Register</button>
    </div>
  </form>
  
  {#if showSuccess}
    <div id="success-message" class="message success">
      {successMessage}
    </div>
  {/if}
  
  {#if showError}
    <div id="error-message" class="message error">
      {errorMessage}
    </div>
  {/if}
</div>

<style>
  .registration-form {
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
    border-color: #007bff;
    box-shadow: 0 0 0 2px rgba(0,123,255,0.25);
  }

  .form-group button {
    width: 100%;
    padding: 10px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 3px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
  }

  .form-group button:hover {
    background-color: #0056b3;
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