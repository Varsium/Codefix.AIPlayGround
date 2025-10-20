// Workflow Canvas JavaScript functionality
window.workflowCanvas = {
    // Get bounding client rect for an element
    getBoundingClientRect: (element) => {
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    },

    // Initialize drag and drop for the canvas
    initializeCanvasDragDrop: (canvasElement) => {
        canvasElement.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.stopPropagation();
            e.dataTransfer.dropEffect = 'copy';
        });

        canvasElement.addEventListener('dragenter', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });

        canvasElement.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            const nodeType = e.dataTransfer.getData('text/plain');
            const rect = canvasElement.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            // Call Blazor method directly
            DotNet.invokeMethodAsync('Codefix.AIPlayGround', 'HandleNodeDrop', nodeType, x, y);
        });
    },

    // Initialize drag and drop for the canvas
    initializeDragDrop: (canvasElement) => {
        canvasElement.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        });

        canvasElement.addEventListener('drop', (e) => {
            e.preventDefault();
            const nodeType = e.dataTransfer.getData('text/plain');
            const rect = canvasElement.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            // Dispatch custom event to Blazor
            const event = new CustomEvent('nodeDropped', {
                detail: { nodeType, x, y }
            });
            canvasElement.dispatchEvent(event);
        });
    },

    // Initialize node dragging
    initializeNodeDrag: (nodeElement, nodeId) => {
        let isDragging = false;
        let startX, startY, initialX, initialY;

        nodeElement.addEventListener('mousedown', (e) => {
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

        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;
            
            const deltaX = e.clientX - startX;
            const deltaY = e.clientY - startY;
            
            const newX = initialX + deltaX;
            const newY = initialY + deltaY;
            
            // Dispatch custom event to Blazor
            const event = new CustomEvent('nodeDragged', {
                detail: { nodeId, x: newX, y: newY }
            });
            nodeElement.dispatchEvent(event);
        });

        document.addEventListener('mouseup', () => {
            if (isDragging) {
                isDragging = false;
                nodeElement.style.cursor = 'move';
                
                // Dispatch final position event
                const event = new CustomEvent('nodeDragEnd', {
                    detail: { nodeId }
                });
                nodeElement.dispatchEvent(event);
            }
        });
    },

    // Initialize connection creation
    initializeConnection: (portElement, nodeId, portType, portIndex) => {
        portElement.addEventListener('mousedown', (e) => {
            e.stopPropagation();
            
            const event = new CustomEvent('connectionStart', {
                detail: { nodeId, portType, portIndex }
            });
            portElement.dispatchEvent(event);
        });
    },

    // Initialize drag for palette items
    initializePaletteDrag: (paletteItem, nodeType) => {
        paletteItem.addEventListener('dragstart', (e) => {
            e.dataTransfer.effectAllowed = 'copy';
            e.dataTransfer.setData('text/plain', nodeType);
        });
    },

    // Copy text to clipboard
    copyToClipboard: (text) => {
        return navigator.clipboard.writeText(text);
    },

    // Show alert
    showAlert: (message) => {
        alert(message);
    },

    // Get bounding client rect
    getBoundingClientRect: (element) => {
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    },

    // Initialize modern drag and drop for the canvas
    initializeModernDragDrop: (canvasElement, dotNetRef) => {
        console.log('Initializing modern drag and drop for canvas:', canvasElement);
        console.log('Canvas element class:', canvasElement.className);
        console.log('Canvas element ID:', canvasElement.id);
        console.log('DotNet reference:', dotNetRef);
        
        // Remove any existing event listeners
        canvasElement.removeEventListener('dragover', handleDragOver);
        canvasElement.removeEventListener('dragleave', handleDragLeave);
        canvasElement.removeEventListener('drop', handleDrop);

        function handleDragOver(e) {
            console.log('Drag over event triggered');
            e.preventDefault();
            e.stopPropagation();
            e.dataTransfer.dropEffect = 'copy';
            dotNetRef.invokeMethodAsync('OnDragOver');
        }

        function handleDragLeave(e) {
            console.log('Drag leave event triggered');
            e.preventDefault();
            e.stopPropagation();
            dotNetRef.invokeMethodAsync('OnDragLeave');
        }

        function handleDrop(e) {
            console.log('Drop event triggered');
            e.preventDefault();
            e.stopPropagation();
            
            // Get the node type from the global variable set by the palette
            const nodeType = window.draggedNodeType || 'StartNode';
            console.log('Dropped node type:', nodeType);
            const rect = canvasElement.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            console.log('Drop position:', x, y);
            
            dotNetRef.invokeMethodAsync('OnDrop', nodeType, x, y);
        }

        // Add event listeners
        canvasElement.addEventListener('dragover', handleDragOver);
        canvasElement.addEventListener('dragleave', handleDragLeave);
        canvasElement.addEventListener('drop', handleDrop);
        
        console.log('Event listeners added to canvas');
        
        // Add global drag event listeners for debugging
        document.addEventListener('dragstart', (e) => {
            console.log('Global drag start detected:', e.target);
        });
        
        document.addEventListener('dragover', (e) => {
            console.log('Global drag over detected:', e.target);
        });
        
        document.addEventListener('drop', (e) => {
            console.log('Global drop detected:', e.target);
        });
    },

    // Set the dragged node type
    setDraggedNodeType: (nodeType) => {
        console.log('Setting dragged node type:', nodeType);
        window.draggedNodeType = nodeType;
    },

    // Test function to manually trigger drag and drop
    testDragDrop: () => {
        console.log('Testing drag and drop...');
        console.log('Current dragged node type:', window.draggedNodeType);
        
        // Simulate a drop event
        const canvas = document.querySelector('.canvas-area');
        if (canvas) {
            console.log('Found canvas element:', canvas);
            const rect = canvas.getBoundingClientRect();
            const x = rect.width / 2;
            const y = rect.height / 2;
            console.log('Simulating drop at:', x, y);
            
            // Create a fake drop event
            const dropEvent = new Event('drop', { bubbles: true });
            dropEvent.clientX = rect.left + x;
            dropEvent.clientY = rect.top + y;
            canvas.dispatchEvent(dropEvent);
        } else {
            console.log('Canvas element not found');
        }
    }
};