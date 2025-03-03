// Get the sidebar element, toggle button, and icon
const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const toggleIcon = document.getElementById('toggleIcon');

// Check localStorage for the sidebar state
const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';

// Apply the initial state (expanded or collapsed)
if (isCollapsed) {
    sidebar.classList.add('collapsed');
    toggleIcon.classList.remove('bi-chevron-double-left');  // Remove expand icon
    toggleIcon.classList.add('bi-chevron-double-right');    // Add collapse icon
} else {
    sidebar.classList.remove('collapsed');
    toggleIcon.classList.remove('bi-chevron-double-right');  // Remove collapse icon
    toggleIcon.classList.add('bi-chevron-double-left');      // Add expand icon
}

// Add event listener to toggle the sidebar
sidebarToggle.addEventListener('click', function () {
    // Toggle the sidebar's collapsed state
    sidebar.classList.toggle('collapsed');

    // Toggle the icon state
    if (sidebar.classList.contains('collapsed')) {
        toggleIcon.classList.remove('bi-chevron-double-left');
        toggleIcon.classList.add('bi-chevron-double-right');
    } else {
        toggleIcon.classList.remove('bi-chevron-double-right');
        toggleIcon.classList.add('bi-chevron-double-left');
    }

    // Save the state to localStorage (persist between page loads)
    const isCollapsedNow = sidebar.classList.contains('collapsed');
    localStorage.setItem('sidebarCollapsed', isCollapsedNow);
});
