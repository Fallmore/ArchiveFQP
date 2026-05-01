window.pdfViewer = {
    scrollToPage: function (pageIndex) {
        const element = document.getElementById('page-' + pageIndex);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
};

// Блокировка контекстного меню
document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
});

// Блокировка горячих клавиш
document.addEventListener('keydown', function (e) {
    // Ctrl+S, Ctrl+P, Ctrl+Shift+I, F12
    if ((e.ctrlKey && (e.key === 's' || e.key === 'p' || e.key === 'u')) ||
        e.key === 'F12' ||
        (e.ctrlKey && e.shiftKey && e.key === 'I')) {
        e.preventDefault();
        return false;
    }
});

// Защита от перетаскивания
document.addEventListener('dragstart', function (e) {
    if (e.target.closest('.page-wrapper')) {
        e.preventDefault();
        return false;
    }
});

// Блокировка сохранения изображения через долгое нажатие на мобильных
document.addEventListener('touchstart', function (e) {
    if (e.target.closest('.page-image')) {
        e.preventDefault();
    }
});