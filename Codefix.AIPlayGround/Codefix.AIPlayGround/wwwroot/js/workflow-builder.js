// Workflow Builder JavaScript Functions

window.downloadFile = function(filename, content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
};

window.loadWorkflowFile = function() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = function(event) {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                const content = e.target.result;
                // This would need to be connected to Blazor component
                console.log('Workflow file loaded:', content);
            };
            reader.readAsText(file);
        }
    };
    input.click();
};

window.getBoundingClientRect = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            width: rect.width,
            height: rect.height
        };
    }
    return { left: 0, top: 0, right: 0, bottom: 0, width: 0, height: 0 };
};

// Drag and drop functionality for palette items
document.addEventListener('DOMContentLoaded', function() {
    const paletteItems = document.querySelectorAll('.palette-item');
    
    paletteItems.forEach(item => {
        item.addEventListener('dragstart', function(e) {
            const nodeType = this.getAttribute('data-node-type');
            e.dataTransfer.setData('text/plain', nodeType);
            e.dataTransfer.effectAllowed = 'copy';
        });
    });
});

// Mermaid initialization
window.initializeMermaid = function() {
    if (typeof mermaid !== 'undefined') {
        mermaid.initialize({
            startOnLoad: true,
            theme: 'default',
            flowchart: {
                useMaxWidth: true,
                htmlLabels: true,
                curve: 'basis'
            }
        });
    }
};

// Re-render Mermaid diagrams
window.renderMermaid = function(elementId) {
    if (typeof mermaid !== 'undefined') {
        const element = document.getElementById(elementId);
        if (element) {
            mermaid.init(undefined, element);
        }
    }
};
