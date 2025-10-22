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

    // Initialize workflow with data from C#
    window.workflowCanvas.initializeWorkflow = function(workflowData) {
        console.log('Initializing workflow with data:', workflowData);
        
        if (!workflowData || !workflowData.nodes) {
            console.warn('No workflow data provided');
            return;
        }
        
        // Clear existing canvas
        if (canvasElement) {
            canvasElement.innerHTML = '';
        }
        
        // Create nodes with positions from C#
        workflowData.nodes.forEach(nodeData => {
            createNode(nodeData);
        });
        
        // Create connections if provided
        if (workflowData.connections && workflowData.connections.length > 0) {
            workflowData.connections.forEach(connectionData => {
                createConnection(connectionData);
            });
        }
        
        // Enable dragging for all nodes
        enableNodeDragging();
    };

    // Create a node with position data
    function createNode(nodeData) {
        if (!canvasElement) return;
        
        const nodeElement = document.createElement('div');
        nodeElement.className = 'workflow-node';
        nodeElement.setAttribute('data-node-id', nodeData.id);
        nodeElement.style.position = 'absolute';
        nodeElement.style.left = nodeData.x + 'px';
        nodeElement.style.top = nodeData.y + 'px';
        nodeElement.style.width = '200px';
        nodeElement.style.backgroundColor = 'white';
        nodeElement.style.border = '2px solid #dee2e6';
        nodeElement.style.borderRadius = '8px';
        nodeElement.style.padding = '10px';
        nodeElement.style.cursor = 'move';
        nodeElement.style.zIndex = '10';
        
        nodeElement.innerHTML = `
            <div class="node-header">
                <strong>${nodeData.name}</strong>
                <small class="text-muted d-block">${nodeData.type}</small>
            </div>
            <div class="node-actions mt-2">
                <button class="btn btn-sm btn-outline-danger" onclick="deleteNode('${nodeData.id}')">
                    <i class="bi bi-trash"></i>
                </button>
            </div>
        `;
        
        canvasElement.appendChild(nodeElement);
        console.log(`Created node: ${nodeData.name} at (${nodeData.x}, ${nodeData.y})`);
    }

    // Create a connection between nodes
    function createConnection(connectionData) {
        if (!canvasElement) return;
        
        const fromNode = canvasElement.querySelector(`[data-node-id="${connectionData.fromNodeId}"]`);
        const toNode = canvasElement.querySelector(`[data-node-id="${connectionData.toNodeId}"]`);
        
        if (!fromNode || !toNode) {
            console.warn('Could not find nodes for connection:', connectionData);
            return;
        }
        
        // Create SVG line for connection
        let svgElement = canvasElement.querySelector('svg');
        if (!svgElement) {
            svgElement = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            svgElement.style.position = 'absolute';
            svgElement.style.top = '0';
            svgElement.style.left = '0';
            svgElement.style.width = '100%';
            svgElement.style.height = '100%';
            svgElement.style.pointerEvents = 'none';
            svgElement.style.zIndex = '1';
            canvasElement.appendChild(svgElement);
        }
        
        const fromRect = fromNode.getBoundingClientRect();
        const toRect = toNode.getBoundingClientRect();
        const canvasRect = canvasElement.getBoundingClientRect();
        
        const fromX = fromRect.left + fromRect.width - canvasRect.left;
        const fromY = fromRect.top + fromRect.height / 2 - canvasRect.top;
        const toX = toRect.left - canvasRect.left;
        const toY = toRect.top + toRect.height / 2 - canvasRect.top;
        
        const line = document.createElementNS('http://www.w3.org/2000/svg', 'line');
        line.setAttribute('x1', fromX);
        line.setAttribute('y1', fromY);
        line.setAttribute('x2', toX);
        line.setAttribute('y2', toY);
        line.setAttribute('stroke', '#6c757d');
        line.setAttribute('stroke-width', '2');
        line.setAttribute('marker-end', 'url(#arrowhead)');
        
        svgElement.appendChild(line);
        
        // Add label if provided
        if (connectionData.label) {
            const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
            const midX = (fromX + toX) / 2;
            const midY = (fromY + toY) / 2 - 5;
            text.setAttribute('x', midX);
            text.setAttribute('y', midY);
            text.setAttribute('text-anchor', 'middle');
            text.setAttribute('font-size', '12');
            text.setAttribute('fill', '#495057');
            text.textContent = connectionData.label;
            svgElement.appendChild(text);
        }
        
        console.log(`Created connection: ${connectionData.fromNodeId} -> ${connectionData.toNodeId} with label: ${connectionData.label}`);
    }

    // Add a new node to the canvas
    window.workflowCanvas.addNode = function(nodeData) {
        createNode(nodeData);
        enableNodeDragging();
    };

    // Clear the canvas
    window.workflowCanvas.clearCanvas = function() {
        if (canvasElement) {
            canvasElement.innerHTML = '';
        }
    };

    // Zoom functions
    window.workflowCanvas.zoomIn = function() {
        if (canvasElement) {
            const currentTransform = canvasElement.style.transform || 'scale(1)';
            const currentScale = parseFloat(currentTransform.match(/scale\(([^)]+)\)/)?.[1] || '1');
            const newScale = Math.min(currentScale * 1.2, 3);
            canvasElement.style.transform = `scale(${newScale})`;
        }
    };

    window.workflowCanvas.zoomOut = function() {
        if (canvasElement) {
            const currentTransform = canvasElement.style.transform || 'scale(1)';
            const currentScale = parseFloat(currentTransform.match(/scale\(([^)]+)\)/)?.[1] || '1');
            const newScale = Math.max(currentScale / 1.2, 0.3);
            canvasElement.style.transform = `scale(${newScale})`;
        }
    };

    window.workflowCanvas.resetZoom = function() {
        if (canvasElement) {
            canvasElement.style.transform = 'scale(1)';
        }
    };

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