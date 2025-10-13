// Enhanced Workflow Canvas JavaScript Support
window.workflowCanvas = {
    // Get bounding client rect for an element
    getBoundingClientRect: function(element) {
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    },

    // Initialize drag and drop for canvas
    initializeDragDrop: function(canvasElement) {
        canvasElement.addEventListener('dragover', function(e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        });

        canvasElement.addEventListener('drop', function(e) {
            e.preventDefault();
            const nodeType = e.dataTransfer.getData('text/plain');
            const rect = canvasElement.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            // Trigger Blazor event
            DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnNodeDropped', nodeType, x, y);
        });
    },

    // Initialize node dragging
    initializeNodeDrag: function(nodeElement) {
        let isDragging = false;
        let startX, startY, initialX, initialY;

        nodeElement.addEventListener('mousedown', function(e) {
            if (e.target.closest('.node-actions')) return; // Don't drag if clicking on actions
            
            isDragging = true;
            startX = e.clientX;
            startY = e.clientY;
            
            const rect = nodeElement.getBoundingClientRect();
            initialX = rect.left;
            initialY = rect.top;
            
            nodeElement.style.cursor = 'grabbing';
            e.preventDefault();
        });

        document.addEventListener('mousemove', function(e) {
            if (!isDragging) return;
            
            const deltaX = e.clientX - startX;
            const deltaY = e.clientY - startY;
            
            nodeElement.style.left = (initialX + deltaX) + 'px';
            nodeElement.style.top = (initialY + deltaY) + 'px';
        });

        document.addEventListener('mouseup', function() {
            if (isDragging) {
                isDragging = false;
                nodeElement.style.cursor = 'grab';
                
                // Get final position
                const rect = nodeElement.getBoundingClientRect();
                const nodeId = nodeElement.dataset.nodeId;
                
                // Trigger Blazor event to update position
                DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnNodeMoved', nodeId, rect.left, rect.top);
            }
        });
    },

    // Initialize connection drawing
    initializeConnectionDrawing: function(canvasElement) {
        let isDrawing = false;
        let startPort = null;
        let tempLine = null;

        canvasElement.addEventListener('mousedown', function(e) {
            const port = e.target.closest('.port');
            if (port && port.classList.contains('output-port')) {
                isDrawing = true;
                startPort = port;
                
                // Create temporary line
                tempLine = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                tempLine.setAttribute('stroke', '#007bff');
                tempLine.setAttribute('stroke-width', '2');
                tempLine.setAttribute('stroke-dasharray', '5,5');
                
                const svg = canvasElement.querySelector('.connection-layer');
                svg.appendChild(tempLine);
                
                const rect = port.getBoundingClientRect();
                const canvasRect = canvasElement.getBoundingClientRect();
                const x = rect.left + rect.width / 2 - canvasRect.left;
                const y = rect.top + rect.height / 2 - canvasRect.top;
                
                tempLine.setAttribute('x1', x);
                tempLine.setAttribute('y1', y);
                tempLine.setAttribute('x2', x);
                tempLine.setAttribute('y2', y);
            }
        });

        canvasElement.addEventListener('mousemove', function(e) {
            if (isDrawing && tempLine) {
                const canvasRect = canvasElement.getBoundingClientRect();
                const x = e.clientX - canvasRect.left;
                const y = e.clientY - canvasRect.top;
                
                tempLine.setAttribute('x2', x);
                tempLine.setAttribute('y2', y);
            }
        });

        canvasElement.addEventListener('mouseup', function(e) {
            if (isDrawing) {
                const targetPort = e.target.closest('.port');
                if (targetPort && targetPort.classList.contains('input-port') && targetPort !== startPort) {
                    // Complete connection
                    const startNodeId = startPort.closest('.workflow-node').dataset.nodeId;
                    const targetNodeId = targetPort.closest('.workflow-node').dataset.nodeId;
                    
                    DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'OnConnectionCreated', startNodeId, targetNodeId);
                }
                
                // Clean up
                if (tempLine) {
                    tempLine.remove();
                    tempLine = null;
                }
                
                isDrawing = false;
                startPort = null;
            }
        });
    },

    // Copy text to clipboard
    copyToClipboard: function(text) {
        navigator.clipboard.writeText(text).then(function() {
            console.log('Text copied to clipboard');
        }).catch(function(err) {
            console.error('Failed to copy text: ', err);
        });
    },

    // Show notification
    showNotification: function(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // Auto remove after 3 seconds
        setTimeout(function() {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    },

    // Initialize all canvas functionality
    initialize: function(canvasElement) {
        this.initializeDragDrop(canvasElement);
        this.initializeConnectionDrawing(canvasElement);
        
        // Initialize drag for all nodes
        const nodes = canvasElement.querySelectorAll('.workflow-node');
        nodes.forEach(node => {
            this.initializeNodeDrag(node);
        });
    }
};

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    const canvas = document.querySelector('.canvas-area');
    if (canvas) {
        window.workflowCanvas.initialize(canvas);
    }
});
