document.addEventListener('DOMContentLoaded', function() {
    const calendarEl = document.getElementById('calendar');
    const appointmentForm = document.getElementById('appointmentForm');
    const refreshButton = document.getElementById('refreshButton');
    const cancelButton = document.getElementById('cancelButton');
    const removeButton = document.getElementById('removeButton');
    const conflictModal = new bootstrap.Modal(document.getElementById('conflictModal'));
    const alternativeTimesList = document.getElementById('alternativeTimes');
    const durationInput = document.getElementById('duration');
    const patientNameInput = document.getElementById('patientName');

    // API base URL - update this to match your API project's URL
    const API_BASE_URL = 'https://localhost:7142/api';

    let selectedTimeSlot = null;

    let calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'timeGridWeek',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        slotMinTime: '00:00:00',
        slotMaxTime: '24:00:00',
        allDaySlot: false,
        slotDuration: '00:15:00',
        selectable: true,
        selectMirror: true,
        dayMaxEvents: true,
        select: function(info) {
            showAppointmentForm();
            document.getElementById('appointmentDate').value = formatDateTime(info.start);
            
            // Calculate duration in minutes from the selection
            const durationMs = info.end - info.start;
            const durationMinutes = Math.round(durationMs / (1000 * 60));
            document.getElementById('duration').value = durationMinutes;
        },
        eventClick: function(info) {
            loadAppointment(info.event.id);
        },
        dateClick: function(info) {
            // If there's no appointment ID, it means we're creating a new appointment
            const appointmentId = document.getElementById('appointmentId').value;
            if (!appointmentId) {
                resetForm();
            }
        },
        events: `${API_BASE_URL}/appointments`
    });

    calendar.render();

    // Prevent form fields from causing calendar to lose focus
    const formInputs = appointmentForm.querySelectorAll('input, select, textarea');
    formInputs.forEach(input => {
        input.addEventListener('focus', function(e) {
            e.stopPropagation();
        });
        input.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    });

    // Handle duration changes
    durationInput.addEventListener('change', function() {
        const appointmentId = document.getElementById('appointmentId').value;
        if (appointmentId) {
            // Update the event in the calendar
            const event = calendar.getEventById(appointmentId);
            if (event) {
                const start = new Date(event.start);
                const newEnd = new Date(start.getTime() + (this.value * 60000));
                event.setEnd(newEnd);
            }
        }
    });

    // Refresh button click handler
    refreshButton.addEventListener('click', function() {
        loadAppointments();
    });

    // Remove button click handler
    removeButton.addEventListener('click', async function() {
        const appointmentId = document.getElementById('appointmentId').value;
        if (!appointmentId) return;

        if (confirm('Are you sure you want to remove this appointment?')) {
            try {
                const response = await fetch(`${API_BASE_URL}/appointments/${appointmentId}`, {
                    method: 'DELETE'
                });

                if (!response.ok) throw new Error('Failed to delete appointment');

                resetForm();
                loadAppointments();
            } catch (error) {
                console.error('Error deleting appointment:', error);
                alert('Error deleting appointment');
            }
        }
    });

    // Load appointments
    async function loadAppointments() {
        try {
            const response = await fetch(`${API_BASE_URL}/appointments`);
            const appointments = await response.json();
            calendar.removeAllEvents();
            appointments.forEach(appointment => {
                calendar.addEvent({
                    id: appointment.id,
                    title: `${appointment.patientName} - ${appointment.healthcareProfessionalName}`,
                    start: appointment.appointmentDate,
                    end: new Date(new Date(appointment.appointmentDate).getTime() + appointment.duration * 60000),
                    description: appointment.description
                });
            });
        } catch (error) {
            console.error('Error loading appointments:', error);
            alert('Error loading appointments');
        }
    }

    // Load single appointment
    async function loadAppointment(id) {
        try {
            const response = await fetch(`${API_BASE_URL}/appointments/${id}`);
            if (!response.ok) throw new Error('Appointment not found');
            
            const appointment = await response.json();
            showAppointmentForm();
            document.getElementById('appointmentId').value = appointment.id;
            document.getElementById('patientName').value = appointment.patientName;
            document.getElementById('healthcareProfessionalName').value = appointment.healthcareProfessionalName;
            
            // Fix time zone handling
            const appointmentDate = new Date(appointment.appointmentDate);
            const localDate = new Date(appointmentDate.getTime() - appointmentDate.getTimezoneOffset() * 60000);
            document.getElementById('appointmentDate').value = localDate.toISOString().slice(0, 16);
            
            document.getElementById('duration').value = appointment.duration;
            document.getElementById('description').value = appointment.description || '';
            
            cancelButton.style.display = 'block';
            removeButton.style.display = 'block';
        } catch (error) {
            console.error('Error loading appointment:', error);
            alert('Error loading appointment');
        }
    }

    // Save appointment
    appointmentForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const appointmentId = document.getElementById('appointmentId').value;
        const appointment = {
            patientName: document.getElementById('patientName').value,
            healthcareProfessionalName: document.getElementById('healthcareProfessionalName').value,
            appointmentDate: document.getElementById('appointmentDate').value,
            duration: parseInt(document.getElementById('duration').value),
            description: document.getElementById('description').value
        };

        try {
            const response = await fetch(`${API_BASE_URL}/appointments${appointmentId ? `/${appointmentId}` : ''}`, {
                method: appointmentId ? 'PUT' : 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(appointment)
            });

            if (response.status === 409) {
                const conflict = await response.json();
                showConflictModal(conflict.alternativeTimes);
                return;
            }

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to save appointment');
            }

            resetForm();
            loadAppointments();
        } catch (error) {
            console.error('Error saving appointment:', error);
            alert(error.message || 'Error saving appointment');
        }
    });

    // Cancel button
    cancelButton.addEventListener('click', async function() {
        resetForm();
        await loadAppointments();
    });

    // Helper functions
    function showAppointmentForm() {
        resetForm();
        appointmentForm.style.display = 'block';
    }

    function resetForm() {
        appointmentForm.reset();
        document.getElementById('appointmentId').value = '';
        cancelButton.style.display = 'none';
        removeButton.style.display = 'none';
    }

    function formatDateTime(date) {
        // Fix time zone handling for new appointments
        const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
        return localDate.toISOString().slice(0, 16);
    }

    function showConflictModal(alternativeTimes) {
        alternativeTimesList.innerHTML = '';
        alternativeTimes.forEach(time => {
            const li = document.createElement('li');
            li.textContent = new Date(time).toLocaleString();
            li.addEventListener('click', () => {
                const localDate = new Date(time);
                document.getElementById('appointmentDate').value = formatDateTime(localDate);
                conflictModal.hide();
            });
            alternativeTimesList.appendChild(li);
        });
        conflictModal.show();
    }

    // Initial load
    loadAppointments();
}); 