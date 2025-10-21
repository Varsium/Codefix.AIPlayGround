// Enhanced Workflow Builder with Drag & Drop Support
window.workflowCanvas = window.workflowCanvas || {};

(function() {
    let draggedNodeType = null;
    let canvasElement = null;
    let dotNetReference = null;
    let isDraggingNode = false;
    let draggedNodeElement = null;
    let dragStartPos = { x: 0, y: 0 };
    let nodeStartPos = { x: 0, y: 0 };

    // Initialize modern drag and drop
    window.workflowCanvas.initializeModernDragDrop = function(canvasRef, dotNetRef) {
        console.log('Initializing modern drag and drop...');
        canvasElement = canvasRef;
        dotNetReference = dotNetRef;

        if (!canvasElement) {
            console.error('Canvas element is null');
            return;
        }

        // Handle palette drag and drop (for creating new nodes)
        canvasElement.addEventListener('dragover', handleDragOver);
        canvasElement.addEventListener('dragleave', handleDragLeave);
        canvasElement.addEventListener('drop', handleDrop);
        
        console.log('Drag and drop initialized successfully');
    };

    function handleDragOver(e) {
        e.preventDefault();
        e.stopPropagation();
        e.dataTransfer.dropEffect = 'copy';
        
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnDragOver');
        }
    }

    function handleDragLeave(e) {
        e.preventDefault();
        e.stopPropagation();
        
        // Only trigger if we're leaving the canvas itself, not child elements
        if (e.target === canvasElement) {
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('OnDragLeave');
            }
        }
    }

    function handleDrop(e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (!draggedNodeType) {
            console.warn('No node type set for drop');
            return;
        }

        const rect = canvasElement.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        console.log(`📦 Dropping NEW node at (${x}, ${y}), type: ${draggedNodeType}`);
        
        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('OnDrop', draggedNodeType, x, y);
        }
        
        draggedNodeType = null;
    }

    // Set the dragged node type from Blazor
    window.workflowCanvas.setDraggedNodeType = function(nodeType) {
        console.log(`Setting dragged node type: ${nodeType}`);
        draggedNodeType = nodeType;
    };

    // Enable node dragging - call this after nodes are rendered
    window.workflowCanvas.enableNodeDragging = function() {
        console.log('Enabling node dragging...');
        const nodes = document.querySelectorAll('.workflow-node');
        console.log(`Found ${nodes.length} nodes`);
        
        nodes.forEach(node => {
            const header = node.querySelector('.node-header');
            if (header) {
                header.style.cursor = 'move';
                // Remove old listener if exists to prevent duplicates
                header.removeEventListener('mousedown', startNodeDrag);
                header.addEventListener('mousedown', startNodeDrag);
                console.log('✅ Added drag handler to node:', node.getAttribute('data-node-id'));
            } else {
                console.warn('❌ No header found for node:', node.getAttribute('data-node-id'));
            }
        });
    };

    function startNodeDrag(e) {
        if (e.button !== 0) return; // Only left mouse button
        
        // Don't start drag if clicking on a button or connection point
        if (e.target.closest('button') || e.target.closest('.connection-point')) {
            return;
        }

        draggedNodeElement = e.target.closest('.workflow-node');
        if (!draggedNodeElement) return;

        e.preventDefault();
        e.stopPropagation();
        isDraggingNode = true;

        const nodeId = draggedNodeElement.getAttribute('data-node-id');
        const currentLeft = parseInt(draggedNodeElement.style.left) || 0;
        const currentTop = parseInt(draggedNodeElement.style.top) || 0;

        dragStartPos = { x: e.clientX, y: e.clientY };
        nodeStartPos = { x: currentLeft, y: currentTop };

        draggedNodeElement.classList.add('dragging');
        
        console.log('🔄 Started dragging existing node:', nodeId);

        document.addEventListener('mousemove', handleNodeDrag);
        document.addEventListener('mouseup', endNodeDrag);
    }

    function handleNodeDrag(e) {
        if (!isDraggingNode || !draggedNodeElement) return;

        e.preventDefault();
        const deltaX = e.clientX - dragStartPos.x;
        const deltaY = e.clientY - dragStartPos.y;

        const newX = nodeStartPos.x + deltaX;
        const newY = nodeStartPos.y + deltaY;

        draggedNodeElement.style.left = newX + 'px';
        draggedNodeElement.style.top = newY + 'px';
    }

    function endNodeDrag(e) {
        if (!isDraggingNode || !draggedNodeElement) return;

        isDraggingNode = false;
        draggedNodeElement.classList.remove('dragging');

        const nodeId = draggedNodeElement.getAttribute('data-node-id');
        const newX = parseInt(draggedNodeElement.style.left) || 0;
        const newY = parseInt(draggedNodeElement.style.top) || 0;

        console.log('🔄 Ended node drag. New position:', newX, newY);

        document.removeEventListener('mousemove', handleNodeDrag);
        document.removeEventListener('mouseup', endNodeDrag);

        // Notify Blazor of position change
        if (dotNetReference && nodeId) {
            console.log('🔄 Notifying Blazor of node move:', nodeId);
            dotNetReference.invokeMethodAsync('OnNodeMoved', nodeId, newX, newY);
        }

        draggedNodeElement = null;
    }

    // Cleanup function
    window.workflowCanvas.cleanup = function() {
        console.log('Cleaning up workflow canvas...');
        
        if (canvasElement) {
            canvasElement.removeEventListener('dragover', handleDragOver);
            canvasElement.removeEventListener('dragleave', handleDragLeave);
            canvasElement.removeEventListener('drop', handleDrop);
        }
        
        document.removeEventListener('mousemove', handleNodeDrag);
        document.removeEventListener('mouseup', endNodeDrag);
        
        canvasElement = null;
        dotNetReference = null;
        draggedNodeType = null;
        isDraggingNode = false;
        draggedNodeElement = null;
    };

})();