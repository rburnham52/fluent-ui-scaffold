using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Example page demonstrating the [Route] attribute with parameterized routes.
    /// This page shows how to create pages that accept route parameters like user IDs.
    /// </summary>
    /// <remarks>
    /// The [Route] attribute defines the URL pattern for this page. Placeholders in curly braces
    /// (like {userId}) will be replaced with actual values when navigating with parameters.
    ///
    /// Usage:
    /// <code>
    /// // Navigate to specific user profile
    /// var userPage = app.NavigateTo&lt;UserPage&gt;(new { userId = "123" });
    /// // Navigates to: http://localhost:5000/users/123
    ///
    /// // Navigate without parameters (placeholder remains)
    /// var userPage = app.NavigateTo&lt;UserPage&gt;();
    /// // Navigates to: http://localhost:5000/users/{userId}
    /// </code>
    /// </remarks>
    [Route("/users/{userId}")]
    public class UserPage : Page<UserPage>
    {
        public IElement UserName { get; private set; } = null!;
        public IElement UserEmail { get; private set; } = null!;
        public IElement EditButton { get; private set; } = null!;
        public IElement BackButton { get; private set; } = null!;

        /// <summary>
        /// Creates a new UserPage instance.
        /// The pageUrl parameter is injected by the framework and contains the resolved URL
        /// (BaseUrl + route path with any parameter substitutions).
        /// </summary>
        public UserPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
            UserName = Element("[data-testid='user-name']")
                .WithDescription("User Name Display")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            UserEmail = Element("[data-testid='user-email']")
                .WithDescription("User Email Display")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            EditButton = Element("[data-testid='edit-user-btn']")
                .WithDescription("Edit User Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            BackButton = Element("[data-testid='back-btn']")
                .WithDescription("Back Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
        }

        /// <summary>
        /// Gets the displayed user name.
        /// </summary>
        public string GetUserName()
        {
            return UserName.GetText();
        }

        /// <summary>
        /// Gets the displayed user email.
        /// </summary>
        public string GetUserEmail()
        {
            return UserEmail.GetText();
        }

        /// <summary>
        /// Clicks the edit button to enter edit mode.
        /// </summary>
        public UserPage ClickEdit()
        {
            return Click(p => p.EditButton);
        }

        /// <summary>
        /// Clicks the back button to return to the previous page.
        /// </summary>
        public UserPage ClickBack()
        {
            return Click(p => p.BackButton);
        }
    }

    /// <summary>
    /// Example page demonstrating nested parameterized routes.
    /// Shows how to define routes with multiple parameters.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <code>
    /// // Navigate to a specific post by a specific user
    /// var postPage = app.NavigateTo&lt;UserPostPage&gt;(new { userId = "123", postId = "456" });
    /// // Navigates to: http://localhost:5000/users/123/posts/456
    /// </code>
    /// </remarks>
    [Route("/users/{userId}/posts/{postId}")]
    public class UserPostPage : Page<UserPostPage>
    {
        public IElement PostTitle { get; private set; } = null!;
        public IElement PostContent { get; private set; } = null!;
        public IElement AuthorName { get; private set; } = null!;

        public UserPostPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
            PostTitle = Element("[data-testid='post-title']")
                .WithDescription("Post Title")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            PostContent = Element("[data-testid='post-content']")
                .WithDescription("Post Content")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            AuthorName = Element("[data-testid='author-name']")
                .WithDescription("Author Name")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
        }

        /// <summary>
        /// Gets the post title.
        /// </summary>
        public string GetPostTitle()
        {
            return PostTitle.GetText();
        }

        /// <summary>
        /// Gets the post content.
        /// </summary>
        public string GetPostContent()
        {
            return PostContent.GetText();
        }
    }

    /// <summary>
    /// Example page demonstrating a simple route without parameters.
    /// Shows the [Route] attribute for static routes.
    /// </summary>
    [Route("/settings")]
    public class SettingsPage : Page<SettingsPage>
    {
        public IElement ThemeToggle { get; private set; } = null!;
        public IElement NotificationsToggle { get; private set; } = null!;
        public IElement SaveButton { get; private set; } = null!;

        public SettingsPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
            ThemeToggle = Element("[data-testid='theme-toggle']")
                .WithDescription("Theme Toggle")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            NotificationsToggle = Element("[data-testid='notifications-toggle']")
                .WithDescription("Notifications Toggle")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            SaveButton = Element("[data-testid='save-settings-btn']")
                .WithDescription("Save Settings Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
        }

        /// <summary>
        /// Toggles the theme setting.
        /// </summary>
        public SettingsPage ToggleTheme()
        {
            return Click(p => p.ThemeToggle);
        }

        /// <summary>
        /// Toggles the notifications setting.
        /// </summary>
        public SettingsPage ToggleNotifications()
        {
            return Click(p => p.NotificationsToggle);
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public SettingsPage SaveSettings()
        {
            return Click(p => p.SaveButton);
        }
    }
}
