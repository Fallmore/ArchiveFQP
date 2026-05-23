// Глобальный объект для уведомлений
window.notifications = {
    // Отображение toast уведомления
    showToast: (title, message, url, duration = 5000) => {
        // Создаем элемент toast
        const toast = document.createElement('div');
        toast.className = 'custom-toast';
        toast.innerHTML = `
            <div class="toast-header">
                <strong class="me-auto">${escapeHtml(title)}</strong>
                <button type="button" class="btn-close" onclick="this.closest('.custom-toast').remove()"></button>
            </div>
            <div class="toast-body">
                ${escapeHtml(message)}
                ${url ? `<br/><small><a href="${url}" onclick="event.preventDefault(); window.location.href='${url}'; this.closest('.custom-toast').remove();">Открыть</a></small>` : ''}
            </div>
        `;
        
        // Добавляем стили
        if (!document.querySelector('#toast-styles')) {
            const styles = document.createElement('style');
            styles.id = 'toast-styles';
            styles.textContent = `
                .custom-toast {
                    position: fixed;
                    bottom: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    background: white;
                    border-radius: 8px;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    animation: slideIn 0.3s ease-out;
                    border-left: 4px solid #007bff;
                }
                .toast-header {
                    padding: 12px;
                    border-bottom: 1px solid #eee;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                }
                .toast-body {
                    padding: 12px;
                }
                .btn-close {
                    background: none;
                    border: none;
                    font-size: 20px;
                    cursor: pointer;
                }
                @keyframes slideIn {
                    from {
                        transform: translateX(100%);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }
                @media (max-width: 768px) {
                    .custom-toast {
                        left: 20px;
                        right: 20px;
                        min-width: auto;
                    }
                }
            `;
            document.head.appendChild(styles);
        }
        
        document.body.appendChild(toast);
        
        // Автоматическое скрытие
        setTimeout(() => {
            if (toast && toast.parentNode) {
                toast.style.animation = 'slideOut 0.3s ease-in';
                setTimeout(() => toast.remove(), 300);
            }
        }, duration);
    },
    
    // Запрос разрешения на уведомления (браузерные)
    requestNotificationPermission: async () => {
        if ('Notification' in window) {
            const permission = await Notification.requestPermission();
            return permission;
        }
        return 'unsupported';
    },
    
    // Отображение уведомления (если разрешено)
    showNotification: (title, message, url) => {
        if ('Notification' in window && Notification.permission === 'granted') {
            const notification = new Notification(title, {
                body: message,
                icon: '/favicon.ico',
                badge: '/badge.png'
            });
            
            notification.onclick = () => {
                window.focus();
                if (url) window.location.href = url;
                notification.close();
            };
        } else {
            window.notifications.showToast(title, message, url);
        }
    },

    showSystemNotification: (title, message, url) => {
            alert(`${title}: ${message}`);
    }
};

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}