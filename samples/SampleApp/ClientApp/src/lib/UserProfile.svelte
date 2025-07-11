<script lang="ts">
  interface UserProfile {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    department: string;
    role: string;
    notifications: {
      email: boolean;
      sms: boolean;
      push: boolean;
    };
    preferences: {
      theme: 'light' | 'dark' | 'auto';
      language: 'en' | 'es' | 'fr';
      timezone: string;
    };
  }

  let profile: UserProfile = {
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    phone: '+1-555-123-4567',
    department: 'Engineering',
    role: 'Senior Developer',
    notifications: {
      email: true,
      sms: false,
      push: true
    },
    preferences: {
      theme: 'light',
      language: 'en',
      timezone: 'America/New_York'
    }
  };

  let isEditing = false;
  let originalProfile: UserProfile;
  let errors: Record<string, string> = {};

  const departments = [
    'Engineering',
    'Marketing',
    'Sales',
    'Human Resources',
    'Finance',
    'Operations'
  ];

  const roles = [
    'Junior Developer',
    'Developer',
    'Senior Developer',
    'Lead Developer',
    'Architect',
    'Manager',
    'Director'
  ];

  const themes = [
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
    { value: 'auto', label: 'Auto' }
  ];

  const languages = [
    { value: 'en', label: 'English' },
    { value: 'es', label: 'Español' },
    { value: 'fr', label: 'Français' }
  ];

  const timezones = [
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'Europe/London',
    'Europe/Paris',
    'Asia/Tokyo'
  ];

  function startEditing() {
    originalProfile = JSON.parse(JSON.stringify(profile));
    isEditing = true;
    errors = {};
  }

  function cancelEditing() {
    profile = originalProfile;
    isEditing = false;
    errors = {};
  }

  function validateForm(): boolean {
    errors = {};

    if (!profile.firstName.trim()) {
      errors.firstName = 'First name is required';
    }

    if (!profile.lastName.trim()) {
      errors.lastName = 'Last name is required';
    }

    if (!profile.email.trim()) {
      errors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(profile.email)) {
      errors.email = 'Please enter a valid email address';
    }

    if (!profile.phone.trim()) {
      errors.phone = 'Phone number is required';
    } else if (!/^\+?[\d\s\-\(\)]+$/.test(profile.phone)) {
      errors.phone = 'Please enter a valid phone number';
    }

    return Object.keys(errors).length === 0;
  }

  function saveProfile() {
    if (validateForm()) {
      // Simulate API call
      setTimeout(() => {
        isEditing = false;
        errors = {};
        // Show success message
        alert('Profile saved successfully!');
      }, 500);
    }
  }

  function resetProfile() {
    if (confirm('Are you sure you want to reset your profile? This action cannot be undone.')) {
      profile = {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        department: 'Engineering',
        role: 'Developer',
        notifications: {
          email: true,
          sms: false,
          push: false
        },
        preferences: {
          theme: 'light',
          language: 'en',
          timezone: 'America/New_York'
        }
      };
      errors = {};
    }
  }

  function toggleNotification(type: keyof typeof profile.notifications) {
    profile.notifications[type] = !profile.notifications[type];
  }
</script>

<div class="profile-container">
  <header class="profile-header">
    <h2>User Profile</h2>
    <div class="profile-actions">
      {#if !isEditing}
        <button on:click={startEditing} data-testid="edit-profile-btn">
          Edit Profile
        </button>
      {:else}
        <button on:click={saveProfile} data-testid="save-profile-btn" class="primary">
          Save Changes
        </button>
        <button on:click={cancelEditing} data-testid="cancel-edit-btn">
          Cancel
        </button>
      {/if}
      <button on:click={resetProfile} data-testid="reset-profile-btn" class="danger">
        Reset Profile
      </button>
    </div>
  </header>

  <div class="profile-content">
    <form class="profile-form" on:submit|preventDefault={saveProfile}>
      <div class="form-section">
        <h3>Personal Information</h3>
        
        <div class="form-row">
          <div class="form-group">
            <label for="firstName">First Name *</label>
            <input
              id="firstName"
              type="text"
              bind:value={profile.firstName}
              disabled={!isEditing}
              data-testid="first-name-input"
              class:error={errors.firstName}
            />
            {#if errors.firstName}
              <span class="error-message" data-testid="first-name-error">{errors.firstName}</span>
            {/if}
          </div>

          <div class="form-group">
            <label for="lastName">Last Name *</label>
            <input
              id="lastName"
              type="text"
              bind:value={profile.lastName}
              disabled={!isEditing}
              data-testid="last-name-input"
              class:error={errors.lastName}
            />
            {#if errors.lastName}
              <span class="error-message" data-testid="last-name-error">{errors.lastName}</span>
            {/if}
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="email">Email Address *</label>
            <input
              id="email"
              type="email"
              bind:value={profile.email}
              disabled={!isEditing}
              data-testid="email-input"
              class:error={errors.email}
            />
            {#if errors.email}
              <span class="error-message" data-testid="email-error">{errors.email}</span>
            {/if}
          </div>

          <div class="form-group">
            <label for="phone">Phone Number *</label>
            <input
              id="phone"
              type="tel"
              bind:value={profile.phone}
              disabled={!isEditing}
              data-testid="phone-input"
              class:error={errors.phone}
            />
            {#if errors.phone}
              <span class="error-message" data-testid="phone-error">{errors.phone}</span>
            {/if}
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>Work Information</h3>
        
        <div class="form-row">
          <div class="form-group">
            <label for="department">Department</label>
            <select
              id="department"
              bind:value={profile.department}
              disabled={!isEditing}
              data-testid="department-select"
            >
              {#each departments as dept}
                <option value={dept}>{dept}</option>
              {/each}
            </select>
          </div>

          <div class="form-group">
            <label for="role">Role</label>
            <select
              id="role"
              bind:value={profile.role}
              disabled={!isEditing}
              data-testid="role-select"
            >
              {#each roles as role}
                <option value={role}>{role}</option>
              {/each}
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>Notification Preferences</h3>
        
        <div class="notification-options">
          <label class="checkbox-label">
            <input
              type="checkbox"
              bind:checked={profile.notifications.email}
              disabled={!isEditing}
              data-testid="email-notification-checkbox"
            />
            <span>Email Notifications</span>
          </label>

          <label class="checkbox-label">
            <input
              type="checkbox"
              bind:checked={profile.notifications.sms}
              disabled={!isEditing}
              data-testid="sms-notification-checkbox"
            />
            <span>SMS Notifications</span>
          </label>

          <label class="checkbox-label">
            <input
              type="checkbox"
              bind:checked={profile.notifications.push}
              disabled={!isEditing}
              data-testid="push-notification-checkbox"
            />
            <span>Push Notifications</span>
          </label>
        </div>
      </div>

      <div class="form-section">
        <h3>Preferences</h3>
        
        <div class="form-row">
          <div class="form-group">
            <label for="theme">Theme</label>
            <select
              id="theme"
              bind:value={profile.preferences.theme}
              disabled={!isEditing}
              data-testid="theme-select"
            >
              {#each themes as theme}
                <option value={theme.value}>{theme.label}</option>
              {/each}
            </select>
          </div>

          <div class="form-group">
            <label for="language">Language</label>
            <select
              id="language"
              bind:value={profile.preferences.language}
              disabled={!isEditing}
              data-testid="language-select"
            >
              {#each languages as lang}
                <option value={lang.value}>{lang.label}</option>
              {/each}
            </select>
          </div>
        </div>

        <div class="form-group">
          <label for="timezone">Timezone</label>
          <select
            id="timezone"
            bind:value={profile.preferences.timezone}
            disabled={!isEditing}
            data-testid="timezone-select"
          >
            {#each timezones as tz}
              <option value={tz}>{tz}</option>
            {/each}
          </select>
        </div>
      </div>
    </form>
  </div>
</div>

<style>
  .profile-container {
    max-width: 800px;
    margin: 0 auto;
  }

  .profile-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #e9ecef;
  }

  .profile-actions {
    display: flex;
    gap: 0.5rem;
  }

  .profile-actions button {
    padding: 0.5rem 1rem;
    border: 1px solid #dee2e6;
    border-radius: 0.25rem;
    background: white;
    cursor: pointer;
    transition: all 0.2s;
  }

  .profile-actions button:hover {
    background: #f8f9fa;
  }

  .profile-actions button.primary {
    background: #007bff;
    color: white;
    border-color: #007bff;
  }

  .profile-actions button.primary:hover {
    background: #0056b3;
  }

  .profile-actions button.danger {
    background: #dc3545;
    color: white;
    border-color: #dc3545;
  }

  .profile-actions button.danger:hover {
    background: #c82333;
  }

  .profile-content {
    background: white;
    border: 1px solid #e9ecef;
    border-radius: 0.5rem;
    padding: 2rem;
  }

  .form-section {
    margin-bottom: 2rem;
  }

  .form-section h3 {
    margin-bottom: 1rem;
    color: #495057;
    border-bottom: 1px solid #e9ecef;
    padding-bottom: 0.5rem;
  }

  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .form-group {
    display: flex;
    flex-direction: column;
  }

  .form-group label {
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #495057;
  }

  .form-group input,
  .form-group select,
  .form-group textarea {
    padding: 0.7rem 1rem;
    border: 1.5px solid #e3e8ee;
    border-radius: 0.375rem;
    background: #fff;
    color: #23272f;
    font-size: 1rem;
    font-weight: 500;
    outline: none;
    transition: border 0.2s, box-shadow 0.2s;
    box-shadow: 0 1px 4px rgba(0,0,0,0.04);
}
.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
    border: 1.5px solid #007bff;
    box-shadow: 0 2px 8px #007bff22;
}
.form-group input::placeholder,
.form-group textarea::placeholder {
    color: #b0b8c1;
    opacity: 1;
}
.form-group input[type="checkbox"],
.checkbox-label input[type="checkbox"] {
    width: 1.2rem;
    height: 1.2rem;
    accent-color: #007bff;
    border-radius: 0.25rem;
    margin-right: 0.5rem;
    vertical-align: middle;
    background: #fff;
    border: 1.5px solid #e3e8ee;
    transition: border 0.2s;
}
.form-group input[type="checkbox"]:focus,
.checkbox-label input[type="checkbox"]:focus {
    border: 1.5px solid #007bff;
}

  .form-group input:disabled,
  .form-group select:disabled {
    background-color: #e9ecef;
    cursor: not-allowed;
  }

  .form-group input.error {
    border-color: #dc3545;
  }

  .error-message {
    color: #dc3545;
    font-size: 0.875rem;
    margin-top: 0.25rem;
  }

  .notification-options {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  .checkbox-label {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
  }

  .checkbox-label:has(input:disabled) {
    opacity: 0.6;
    cursor: not-allowed;
  }

  @media (max-width: 768px) {
    .form-row {
      grid-template-columns: 1fr;
    }

    .profile-header {
      flex-direction: column;
      gap: 1rem;
      align-items: stretch;
    }

    .profile-actions {
      justify-content: center;
    }
  }
</style> 