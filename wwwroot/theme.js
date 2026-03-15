window.LLMThinkTank = window.LLMThinkTank || {};

window.LLMThinkTank.setTheme = (mode) => {
    const root = document.documentElement;
    root.setAttribute('data-theme', mode);
};

window.LLMThinkTank.setControlHeight = (px) => {
    document.documentElement.style.setProperty('--control-height', px + 'px');
};
