# Task 2 - Frontend

Tool: Google Gemini (gemini.google.com), 2026-10-02

## Prompt

```text
Act as a frontend engineer. Build an Event Catalog and Registration Form for an Online Campus Event Management System using plain HTML, CSS and JavaScript. Use semantic HTML5 tags (<header>, <main>, <section>, <article>, <footer>) instead of generic div wrappers. Follow WCAG (POUR): visible <label> for every input, aria-label attributes on input fields, alt text on images, and accessible color contrast. Do not use any frontend framework.
```

## Gemini output

I will now generate the complete, single-file solution for the Online Campus Event Management System. This file includes the HTML structure using semantic tags, accessible CSS styling without any external frameworks, and plain JavaScript to handle the interactive behaviors you specified.

Canvas file: Campus Event Hub

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Campus Event Hub - Online Event Management</title>
    <style>
        /* CSS Reset and Base Styles */
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        :root {
            --primary-bg: #ffffff;
            --secondary-bg: #f4f6f8;
            --text-main: #1a1a1a;
            --text-muted: #4a4a4a;
            --brand-color: #004085; /* High contrast dark blue */
            --brand-hover: #002752;
            --border-color: #cccccc;
            --focus-ring: #d9381e; /* High visibility red for focus state */
            --success-bg: #d4edda;
            --success-text: #155724;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            background-color: var(--secondary-bg);
            color: var(--text-main);
            line-height: 1.6;
        }

        /* Accessibility: Clear Focus States */
        *:focus-visible {
            outline: 3px solid var(--focus-ring);
            outline-offset: 3px;
        }

        /* Layout */
        .container {
            width: 100%;
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1.5rem;
        }

        /* Header & Navigation */
        header {
            background-color: var(--brand-color);
            color: #ffffff;
            padding: 1.5rem 0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        header .container {
            display: flex;
            flex-wrap: wrap;
            justify-content: space-between;
            align-items: center;
        }

        header h1 {
            font-size: 1.8rem;
            margin-right: 1rem;
        }

        nav ul {
            list-style: none;
            display: flex;
            gap: 1.5rem;
        }

        nav a {
            color: #ffffff;
            text-decoration: none;
            font-weight: 600;
            padding: 0.5rem;
            border-radius: 4px;
        }

        nav a:hover, nav a:focus {
            text-decoration: underline;
            background-color: var(--brand-hover);
        }

        /* Main Content Areas */
        main {
            padding: 3rem 0;
        }

        section {
            margin-bottom: 4rem;
        }

        .section-title {
            font-size: 2rem;
            margin-bottom: 2rem;
            color: var(--text-main);
            border-bottom: 2px solid var(--brand-color);
            padding-bottom: 0.5rem;
            display: inline-block;
        }

        /* Event Catalog */
        .event-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
            gap: 2rem;
        }

        article.event-card {
            background-color: var(--primary-bg);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        article.event-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 8px 16px rgba(0,0,0,0.1);
        }

        .event-image {
            width: 100%;
            height: 200px;
            object-fit: cover;
            border-bottom: 1px solid var(--border-color);
        }

        .event-content {
            padding: 1.5rem;
            display: flex;
            flex-direction: column;
            flex-grow: 1;
        }

        .event-title {
            font-size: 1.4rem;
            margin-bottom: 0.5rem;
        }

        .event-meta {
            font-size: 0.95rem;
            color: var(--text-muted);
            margin-bottom: 1rem;
        }

        .event-meta strong {
            color: var(--text-main);
        }

        .event-desc {
            margin-bottom: 1.5rem;
            flex-grow: 1;
        }

        button.btn-primary {
            background-color: var(--brand-color);
            color: #ffffff;
            border: none;
            padding: 0.75rem 1.5rem;
            font-size: 1rem;
            font-weight: 600;
            border-radius: 4px;
            cursor: pointer;
            text-align: center;
            width: 100%;
            transition: background-color 0.2s ease;
        }

        button.btn-primary:hover {
            background-color: var(--brand-hover);
        }

        /* Registration Form */
        .form-wrapper {
            background-color: var(--primary-bg);
            padding: 2.5rem;
            border-radius: 8px;
            border: 1px solid var(--border-color);
            max-width: 600px;
            margin: 0 auto;
        }

        form .form-group {
            margin-bottom: 1.5rem;
        }

        form label {
            display: block;
            font-weight: 600;
            margin-bottom: 0.5rem;
            color: var(--text-main);
        }

        form input, form select {
            width: 100%;
            padding: 0.75rem;
            border: 1px solid var(--border-color);
            border-radius: 4px;
            font-size: 1rem;
            font-family: inherit;
        }

        /* Live Region Message */
        .message-area {
            margin-top: 1.5rem;
            padding: 1rem;
            border-radius: 4px;
            display: none; /* Hidden by default */
        }

        .message-area.success {
            display: block;
            background-color: var(--success-bg);
            color: var(--success-text);
            border: 1px solid #c3e6cb;
        }

        /* Footer */
        footer {
            background-color: #2c2c2c;
            color: #ffffff;
            text-align: center;
            padding: 2rem 0;
            margin-top: auto;
        }

        footer p {
            margin-bottom: 0.5rem;
        }

        footer a {
            color: #add8e6;
            text-decoration: underline;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            header .container {
                flex-direction: column;
                text-align: center;
            }
            nav ul {
                margin-top: 1rem;
                flex-wrap: wrap;
                justify-content: center;
            }
            .form-wrapper {
                padding: 1.5rem;
            }
        }
    </style>
</head>
<body>
    <header>
        <div class="container">
            <h1>Campus Event Hub</h1>
            <nav aria-label="Main Navigation">
                <ul>
                    <li><a href="#events-section" aria-label="Jump to Event Catalog">Events Catalog</a></li>
                    <li><a href="#registration-section" aria-label="Jump to Registration Form">Register</a></li>
                </ul>
            </nav>
        </div>
    </header>

    <main class="container">
        <section id="events-section" aria-labelledby="catalog-title">
            <h2 id="catalog-title" class="section-title">Upcoming Events</h2>

            <div class="event-grid">
                <!-- Event 1 -->
                <article class="event-card">
                    <img src="https://placehold.co/600x400/004085/ffffff?text=Tech+Symposium+2026" alt="A stage with a presentation screen displaying 'Tech Symposium 2026'" class="event-image">
                    <div class="event-content">
                        <h3 class="event-title">Annual Tech Symposium</h3>
                        <p class="event-meta">
                            <strong>Date:</strong> November 15, 2026<br>
                            <strong>Time:</strong> 9:00 AM - 4:00 PM<br>
                            <strong>Location:</strong> Main Auditorium
                        </p>
                        <p class="event-desc">Join industry leaders and campus innovators for a day of deep dives into AI, web development, and cybersecurity trends.</p>
                        <button class="btn-primary register-trigger" data-event-value="tech-symposium" aria-label="Register Now for Annual Tech Symposium">Register Now</button>
                    </div>
                </article>

                <!-- Event 2 -->
                <article class="event-card">
                    <img src="https://placehold.co/600x400/004085/ffffff?text=Career+Fair" alt="Students engaging with recruiters at various company booths during a career fair" class="event-image">
                    <div class="event-content">
                        <h3 class="event-title">Fall Career Fair</h3>
                        <p class="event-meta">
                            <strong>Date:</strong> November 22, 2026<br>
                            <strong>Time:</strong> 10:00 AM - 3:00 PM<br>
                            <strong>Location:</strong> Student Union Building
                        </p>
                        <p class="event-desc">Connect with over 50 top employers actively recruiting for internships and full-time roles across all major disciplines.</p>
                        <button class="btn-primary register-trigger" data-event-value="career-fair" aria-label="Register Now for Fall Career Fair">Register Now</button>
                    </div>
                </article>

                <!-- Event 3 -->
                <article class="event-card">
                    <img src="https://placehold.co/600x400/004085/ffffff?text=Alumni+Mixer" alt="A well-lit banquet hall with people holding drinks and chatting" class="event-image">
                    <div class="event-content">
                        <h3 class="event-title">Alumni Networking Mixer</h3>
                        <p class="event-meta">
                            <strong>Date:</strong> December 5, 2026<br>
                            <strong>Time:</strong> 6:00 PM - 9:00 PM<br>
                            <strong>Location:</strong> Campus Conference Center
                        </p>
                        <p class="event-desc">An evening of networking and mentorship. Meet distinguished alumni, share experiences, and build lifelong professional connections.</p>
                        <button class="btn-primary register-trigger" data-event-value="alumni-mixer" aria-label="Register Now for Alumni Networking Mixer">Register Now</button>
                    </div>
                </article>
            </div>
        </section>

        <section id="registration-section" aria-labelledby="form-title">
            <h2 id="form-title" class="section-title">Event Registration</h2>

            <div class="form-wrapper">
                <form id="event-form" aria-label="Event Registration Form">
                    <div class="form-group">
                        <label for="full-name">Full Name (Required)</label>
                        <input type="text" id="full-name" name="fullName" required aria-required="true" autocomplete="name" placeholder="e.g. Jane Doe">
                    </div>

                    <div class="form-group">
                        <label for="student-id">Student ID Number (Required)</label>
                        <input type="text" id="student-id" name="studentId" required aria-required="true" placeholder="e.g. 123456789">
                    </div>

                    <div class="form-group">
                        <label for="email-address">Email Address (Required)</label>
                        <input type="email" id="email-address" name="emailAddress" required aria-required="true" autocomplete="email" placeholder="e.g. jane.doe@campus.edu">
                    </div>

                    <div class="form-group">
                        <label for="event-selection">Select Event (Required)</label>
                        <select id="event-selection" name="eventSelection" required aria-required="true">
                            <option value="" disabled selected>-- Choose an Event --</option>
                            <option value="tech-symposium">Annual Tech Symposium (Nov 15)</option>
                            <option value="career-fair">Fall Career Fair (Nov 22)</option>
                            <option value="alumni-mixer">Alumni Networking Mixer (Dec 5)</option>
                        </select>
                    </div>

                    <button type="submit" class="btn-primary" aria-label="Submit Registration">Submit Registration</button>
                </form>

                <!-- Live region to announce successful form submission to screen readers -->
                <div id="status-message" class="message-area" aria-live="polite" role="status"></div>
            </div>
        </section>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Campus Event Hub. All rights reserved.</p>
            <p>
                <small>
                    <strong>Accessibility Statement:</strong> We are committed to ensuring this platform is accessible to all users.
                    If you experience any barriers, please <a href="#contact" aria-label="Contact Accessibility Support">contact support</a>.
                </small>
            </p>
        </div>
    </footer>

    <script>
        document.addEventListener('DOMContentLoaded', () => {
            // Grab essential DOM elements
            const registerButtons = document.querySelectorAll('.register-trigger');
            const registrationSection = document.getElementById('registration-section');
            const eventDropdown = document.getElementById('event-selection');
            const registrationForm = document.getElementById('event-form');
            const statusMessage = document.getElementById('status-message');
            const fullNameInput = document.getElementById('full-name');

            // Handle "Register Now" button clicks in the catalog
            registerButtons.forEach(button => {
                button.addEventListener('click', (e) => {
                    const selectedEventValue = e.target.getAttribute('data-event-value');

                    // Smooth scroll to the registration section
                    registrationSection.scrollIntoView({ behavior: 'smooth' });

                    // Pre-fill the dropdown
                    eventDropdown.value = selectedEventValue;

                    // Clear any previous success messages
                    statusMessage.classList.remove('success');
                    statusMessage.textContent = '';

                    // Move focus to the first input in the form for accessibility
                    // A slight timeout ensures the scroll animation doesn't interfere heavily with focus outline drawing
                    setTimeout(() => {
                        fullNameInput.focus();
                    }, 500);
                });
            });

            // Handle form submission
            registrationForm.addEventListener('submit', (e) => {
                // Prevent the default form submission (page reload)
                e.preventDefault();

                // Get values for personalized message (Optional, for better UX)
                const userName = document.getElementById('full-name').value;
                const eventText = eventDropdown.options[eventDropdown.selectedIndex].text;

                // Simulate processing...

                // Display success message in the polite aria-live region
                statusMessage.textContent = `Thank you, ${userName}! Your registration for "${eventText}" was completely successful. A confirmation email has been sent to you.`;
                statusMessage.classList.add('success');

                // Reset the form fields
                registrationForm.reset();
            });
        });
    </script>
</body>
</html>

```
