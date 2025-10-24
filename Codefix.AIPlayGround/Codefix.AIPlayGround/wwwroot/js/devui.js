// CODEFIXUI JavaScript functionality
window.devUI = {
    // Scroll to bottom of logs container
    scrollToBottom: function(element) {
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    },

    // Initialize CODEFIXUI components
    initialize: function() {
        this.initializeDragAndDrop();
        this.initializeCanvas();
        this.initializeCharts();
        this.initializeTooltips();
        this.initializeWorkflowVisualization();
        this.initializeTheme();
    },

    // Initialize drag and drop functionality
    initializeDragAndDrop: function() {
        const canvas = document.querySelector('.flow-canvas');
        if (!canvas) return;

        canvas.addEventListener('dragover', function(e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        });

        canvas.addEventListener('drop', function(e) {
            e.preventDefault();
            const nodeType = e.dataTransfer.getData('text/plain');
            const rect = canvas.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            // Call Blazor method
            DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnDrop', nodeType, x, y);
        });
    },

    // Initialize canvas functionality
    initializeCanvas: function() {
        const canvas = document.querySelector('.flow-canvas');
        if (!canvas) return;

        let isPanning = false;
        let startX, startY;
        let translateX = 0, translateY = 0;

        canvas.addEventListener('mousedown', function(e) {
            if (e.target === canvas) {
                isPanning = true;
                startX = e.clientX;
                startY = e.clientY;
                canvas.style.cursor = 'grabbing';
            }
        });

        canvas.addEventListener('mousemove', function(e) {
            if (isPanning) {
                const deltaX = e.clientX - startX;
                const deltaY = e.clientY - startY;
                translateX += deltaX;
                translateY += deltaY;
                
                canvas.style.transform = `translate(${translateX}px, ${translateY}px)`;
                
                startX = e.clientX;
                startY = e.clientY;
            }
        });

        canvas.addEventListener('mouseup', function() {
            isPanning = false;
            canvas.style.cursor = 'grab';
        });

        canvas.addEventListener('wheel', function(e) {
            e.preventDefault();
            const scale = e.deltaY > 0 ? 0.9 : 1.1;
            const currentScale = parseFloat(canvas.style.transform.match(/scale\(([^)]+)\)/) || [1, 1])[1];
            const newScale = Math.max(0.1, Math.min(3, currentScale * scale));
            
            canvas.style.transform = `translate(${translateX}px, ${translateY}px) scale(${newScale})`;
        });
    },

    // Initialize charts
    initializeCharts: function() {
        this.initializeLineCharts();
        this.initializePieCharts();
        this.initializeHistograms();
    },

    // Initialize line charts
    initializeLineCharts: function() {
        const lineCharts = document.querySelectorAll('.line-chart');
        lineCharts.forEach(chart => {
            this.drawLineChart(chart);
        });
    },

    // Draw line chart
    drawLineChart: function(container) {
        const points = container.querySelectorAll('.chart-point');
        if (points.length < 2) return;

        const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.setAttribute('width', '100%');
        svg.setAttribute('height', '100%');
        svg.style.position = 'absolute';
        svg.style.top = '0';
        svg.style.left = '0';
        svg.style.pointerEvents = 'none';

        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('stroke', '#0078d4');
        path.setAttribute('stroke-width', '2');
        path.setAttribute('fill', 'none');

        let pathData = '';
        points.forEach((point, index) => {
            const x = parseFloat(point.style.left);
            const y = parseFloat(point.style.bottom);
            
            if (index === 0) {
                pathData += `M ${x} ${y}`;
            } else {
                pathData += ` L ${x} ${y}`;
            }
        });

        path.setAttribute('d', pathData);
        svg.appendChild(path);
        container.appendChild(svg);
    },

    // Initialize pie charts
    initializePieCharts: function() {
        const pieCharts = document.querySelectorAll('.pie-chart');
        pieCharts.forEach(chart => {
            this.drawPieChart(chart);
        });
    },

    // Draw pie chart
    drawPieChart: function(container) {
        const slices = container.querySelectorAll('.pie-slice');
        if (slices.length === 0) return;

        const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.setAttribute('width', '100%');
        svg.setAttribute('height', '100%');
        svg.style.position = 'absolute';
        svg.style.top = '0';
        svg.style.left = '0';

        const centerX = 60;
        const centerY = 60;
        const radius = 50;
        let currentAngle = 0;

        slices.forEach(slice => {
            const percentage = parseFloat(slice.style.getPropertyValue('--percentage'));
            const color = slice.style.getPropertyValue('--color');
            const angle = (percentage / 100) * 360;

            const startAngle = currentAngle;
            const endAngle = currentAngle + angle;

            const startX = centerX + radius * Math.cos((startAngle - 90) * Math.PI / 180);
            const startY = centerY + radius * Math.sin((startAngle - 90) * Math.PI / 180);
            const endX = centerX + radius * Math.cos((endAngle - 90) * Math.PI / 180);
            const endY = centerY + radius * Math.sin((endAngle - 90) * Math.PI / 180);

            const largeArcFlag = angle > 180 ? 1 : 0;

            const pathData = `M ${centerX} ${centerY} L ${startX} ${startY} A ${radius} ${radius} 0 ${largeArcFlag} 1 ${endX} ${endY} Z`;

            const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
            path.setAttribute('d', pathData);
            path.setAttribute('fill', color);
            path.setAttribute('stroke', '#1e1e1e');
            path.setAttribute('stroke-width', '2');

            svg.appendChild(path);
            currentAngle += angle;
        });

        container.appendChild(svg);
    },

    // Initialize histograms
    initializeHistograms: function() {
        const histograms = document.querySelectorAll('.histogram');
        histograms.forEach(histogram => {
            this.animateHistogram(histogram);
        });
    },

    // Animate histogram bars
    animateHistogram: function(container) {
        const bars = container.querySelectorAll('.histogram-bar');
        bars.forEach((bar, index) => {
            const height = bar.style.height;
            bar.style.height = '0%';
            bar.style.transition = 'height 0.5s ease';
            
            setTimeout(() => {
                bar.style.height = height;
            }, index * 50);
        });
    },

    // Initialize tooltips
    initializeTooltips: function() {
        const tooltipElements = document.querySelectorAll('[title]');
        tooltipElements.forEach(element => {
            element.addEventListener('mouseenter', this.showTooltip);
            element.addEventListener('mouseleave', this.hideTooltip);
        });
    },

    // Show tooltip
    showTooltip: function(e) {
        const tooltip = document.createElement('div');
        tooltip.className = 'devui-tooltip';
        tooltip.textContent = e.target.getAttribute('title');
        tooltip.style.cssText = `
            position: absolute;
            background: #2d2d30;
            color: #ffffff;
            padding: 8px 12px;
            border-radius: 4px;
            font-size: 12px;
            z-index: 1000;
            pointer-events: none;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            border: 1px solid #3e3e42;
        `;

        document.body.appendChild(tooltip);

        const rect = e.target.getBoundingClientRect();
        tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
        tooltip.style.top = rect.top - tooltip.offsetHeight - 8 + 'px';

        e.target._tooltip = tooltip;
    },

    // Hide tooltip
    hideTooltip: function(e) {
        if (e.target._tooltip) {
            document.body.removeChild(e.target._tooltip);
            delete e.target._tooltip;
        }
    },

    // Export data as JSON
    exportData: function(data, filename) {
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    // Copy to clipboard
    copyToClipboard: function(text) {
        navigator.clipboard.writeText(text).then(() => {
            this.showNotification('Copied to clipboard', 'success');
        }).catch(() => {
            this.showNotification('Failed to copy to clipboard', 'error');
        });
    },

    // Show notification
    showNotification: function(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `devui-notification devui-notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#0078d4'};
            color: white;
            padding: 12px 16px;
            border-radius: 4px;
            z-index: 10000;
            font-size: 14px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transition = 'opacity 0.3s ease';
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    document.body.removeChild(notification);
                }
            }, 300);
        }, 3000);
    },

    // Format bytes
    formatBytes: function(bytes, decimals = 2) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const dm = decimals < 0 ? 0 : decimals;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
    },

    // Format duration
    formatDuration: function(milliseconds) {
        if (milliseconds < 1000) return milliseconds + 'ms';
        if (milliseconds < 60000) return (milliseconds / 1000).toFixed(1) + 's';
        if (milliseconds < 3600000) return (milliseconds / 60000).toFixed(1) + 'm';
        return (milliseconds / 3600000).toFixed(1) + 'h';
    },

    // Debounce function
    debounce: function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Throttle function
    throttle: function(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    // Initialize workflow visualization
    initializeWorkflowVisualization: function() {
        this.setupWorkflowCanvas();
        this.setupNodeInteractions();
        this.setupConnectionAnimations();
    },

    // Setup workflow canvas
    setupWorkflowCanvas: function() {
        const canvas = document.querySelector('.flow-canvas');
        if (!canvas) return;

        // Add zoom and pan functionality
        let isPanning = false;
        let startX, startY;
        let translateX = 0, translateY = 0;
        let scale = 1;

        canvas.addEventListener('mousedown', (e) => {
            if (e.target === canvas) {
                isPanning = true;
                startX = e.clientX;
                startY = e.clientY;
                canvas.style.cursor = 'grabbing';
            }
        });

        canvas.addEventListener('mousemove', (e) => {
            if (isPanning) {
                const deltaX = e.clientX - startX;
                const deltaY = e.clientY - startY;
                translateX += deltaX;
                translateY += deltaY;
                
                canvas.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale})`;
                
                startX = e.clientX;
                startY = e.clientY;
            }
        });

        canvas.addEventListener('mouseup', () => {
            isPanning = false;
            canvas.style.cursor = 'grab';
        });

        canvas.addEventListener('wheel', (e) => {
            e.preventDefault();
            const scaleFactor = e.deltaY > 0 ? 0.9 : 1.1;
            scale = Math.max(0.1, Math.min(3, scale * scaleFactor));
            canvas.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale})`;
        });
    },

    // Setup node interactions
    setupNodeInteractions: function() {
        const nodes = document.querySelectorAll('.flow-node');
        nodes.forEach(node => {
            node.addEventListener('click', (e) => {
                e.stopPropagation();
                this.selectNode(node);
            });

            node.addEventListener('mouseenter', (e) => {
                this.highlightNode(node);
            });

            node.addEventListener('mouseleave', (e) => {
                this.unhighlightNode(node);
            });
        });
    },

    // Select node
    selectNode: function(node) {
        // Remove previous selection
        document.querySelectorAll('.flow-node.selected').forEach(n => {
            n.classList.remove('selected');
        });
        
        // Add selection to current node
        node.classList.add('selected');
        
        // Trigger Blazor event
        if (window.DotNet) {
            DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnNodeSelected', node.dataset.nodeId);
        }
    },

    // Highlight node
    highlightNode: function(node) {
        node.style.transform = 'scale(1.05)';
        node.style.zIndex = '10';
    },

    // Unhighlight node
    unhighlightNode: function(node) {
        if (!node.classList.contains('selected')) {
            node.style.transform = 'scale(1)';
            node.style.zIndex = '1';
        }
    },

    // Setup connection animations
    setupConnectionAnimations: function() {
        const connections = document.querySelectorAll('.connection');
        connections.forEach(connection => {
            connection.addEventListener('mouseenter', () => {
                connection.style.strokeWidth = '4';
                connection.style.stroke = '#0078d4';
            });

            connection.addEventListener('mouseleave', () => {
                connection.style.strokeWidth = '3';
                connection.style.stroke = '#3e3e42';
            });
        });
    },

    // Initialize theme
    initializeTheme: function() {
        const savedTheme = localStorage.getItem('codefixui-theme');
        if (savedTheme) {
            this.applyTheme(savedTheme === 'dark');
        }
    },

    // Toggle theme
    toggleTheme: function(isDark) {
        this.applyTheme(isDark);
        localStorage.setItem('codefixui-theme', isDark ? 'dark' : 'light');
    },

    // Apply theme
    applyTheme: function(isDark) {
        const root = document.documentElement;
        if (isDark) {
            root.setAttribute('data-theme', 'dark');
        } else {
            root.setAttribute('data-theme', 'light');
        }
    },

    // Import session
    importSession: function() {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.json';
        input.onchange = (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    try {
                        const data = JSON.parse(e.target.result);
                        this.showNotification('Session imported successfully!', 'success');
                        // Trigger Blazor event to load session data
                        if (window.DotNet) {
                            DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnSessionImported', data);
                        }
                    } catch (error) {
                        this.showNotification('Failed to import session: Invalid file format', 'error');
                    }
                };
                reader.readAsText(file);
            }
        };
        input.click();
    }
};

// Initialize CODEFIXUI when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    if (window.devUI && typeof window.devUI.initialize === 'function') {
        window.devUI.initialize();
    } else {
        console.warn('devUI not available on DOMContentLoaded');
    }
});

// Also try to initialize when the window loads
window.addEventListener('load', function() {
    if (window.devUI && typeof window.devUI.initialize === 'function') {
        window.devUI.initialize();
    }
});

// Export for global access
window.scrollToBottom = window.devUI.scrollToBottom;
