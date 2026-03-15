window.LLMThinkTank = window.LLMThinkTank || {};

window.LLMThinkTank.setTheme = (mode) => {
    const root = document.documentElement;
    root.setAttribute('data-theme', mode);
};

window.LLMThinkTank.setControlHeight = (px) => {
    document.documentElement.style.setProperty('--control-height', px + 'px');
};

window.LLMThinkTank.setGutter = (px) => {
    document.documentElement.style.setProperty('--gutter', px + 'px');
};

window.LLMThinkTank.setBorderRadius = (px) => {
    document.documentElement.style.setProperty('--radius', px + 'px');
};
