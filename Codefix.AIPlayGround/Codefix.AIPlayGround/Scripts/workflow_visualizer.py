#!/usr/bin/env python3
"""
Microsoft Agent Framework Workflow Visualizer
Python script for generating workflow diagrams
"""

import json
import sys
from typing import Dict, List, Any, Optional

def generate_mermaid_diagram(workflow_data: Dict[str, Any]) -> str:
    """Generate Mermaid diagram from workflow data"""
    mermaid = ['graph TD']
    
    # Add nodes
    for node in workflow_data.get('nodes', []):
        node_id = node.get('id', 'unknown')
        node_name = node.get('name', 'Unknown')
        node_type = node.get('type', 'executor')
        
        # Format node based on type
        if node_type == 'start':
            mermaid.append(f'    {node_id}["🚀 {node_name}"]')
        elif node_type == 'end':
            mermaid.append(f'    {node_id}["🏁 {node_name}"]')
        elif node_type == 'agent':
            mermaid.append(f'    {node_id}["🤖 {node_name}"]')
        elif node_type == 'function':
            mermaid.append(f'    {node_id}["⚙️ {node_name}"]')
        elif node_type == 'condition':
            mermaid.append(f'    {node_id}["🔀 {node_name}"]')
        else:
            mermaid.append(f'    {node_id}["📦 {node_name}"]')
    
    # Add edges
    for edge in workflow_data.get('edges', []):
        from_node = edge.get('from', 'unknown')
        to_node = edge.get('to', 'unknown')
        label = edge.get('label', '')
        
        if label:
            mermaid.append(f'    {from_node} -->|{label}| {to_node}')
        else:
            mermaid.append(f'    {from_node} --> {to_node}')
    
    # Add styling
    mermaid.append('')
    mermaid.append('    %% Styling')
    mermaid.append('    classDef startNode fill:#90EE90,stroke:#333,stroke-width:2px')
    mermaid.append('    classDef endNode fill:#FFB6C1,stroke:#333,stroke-width:2px')
    mermaid.append('    classDef agentNode fill:#87CEEB,stroke:#333,stroke-width:2px')
    mermaid.append('    classDef functionNode fill:#FFE4B5,stroke:#333,stroke-width:2px')
    mermaid.append('    classDef conditionNode fill:#DDA0DD,stroke:#333,stroke-width:2px')
    
    # Apply styles
    for node in workflow_data.get('nodes', []):
        node_id = node.get('id', 'unknown')
        node_type = node.get('type', 'executor')
        
        if node_type == 'start':
            mermaid.append(f'    class {node_id} startNode')
        elif node_type == 'end':
            mermaid.append(f'    class {node_id} endNode')
        elif node_type == 'agent':
            mermaid.append(f'    class {node_id} agentNode')
        elif node_type == 'function':
            mermaid.append(f'    class {node_id} functionNode')
        elif node_type == 'condition':
            mermaid.append(f'    class {node_id} conditionNode')
    
    return '\n'.join(mermaid)

def generate_dot_diagram(workflow_data: Dict[str, Any]) -> str:
    """Generate GraphViz DOT diagram from workflow data"""
    dot = ['digraph Workflow {']
    dot.append('    rankdir=LR;')
    dot.append('    node [shape=box, style=rounded];')
    dot.append('')
    
    # Add nodes
    for node in workflow_data.get('nodes', []):
        node_id = node.get('id', 'unknown')
        node_name = node.get('name', 'Unknown')
        node_type = node.get('type', 'executor')
        
        # Format node based on type
        if node_type == 'start':
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=lightgreen, style="filled,rounded"];')
        elif node_type == 'end':
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=lightcoral, style="filled,rounded"];')
        elif node_type == 'agent':
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=lightblue, style="filled,rounded"];')
        elif node_type == 'function':
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=lightyellow, style="filled,rounded"];')
        elif node_type == 'condition':
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=plum, style="filled,rounded"];')
        else:
            dot.append(f'    {node_id} [label="{node_name}", fillcolor=lightgray, style="filled,rounded"];')
    
    dot.append('')
    
    # Add edges
    for edge in workflow_data.get('edges', []):
        from_node = edge.get('from', 'unknown')
        to_node = edge.get('to', 'unknown')
        label = edge.get('label', '')
        
        if label:
            dot.append(f'    {from_node} -> {to_node} [label="{label}"];')
        else:
            dot.append(f'    {from_node} -> {to_node};')
    
    dot.append('}')
    return '\n'.join(dot)

def create_sample_workflow() -> Dict[str, Any]:
    """Create a sample workflow for demonstration"""
    return {
        "name": "Research and Analysis Workflow",
        "nodes": [
            {"id": "start", "name": "Start", "type": "start"},
            {"id": "research", "name": "Research Agent", "type": "agent"},
            {"id": "analysis", "name": "Analysis Agent", "type": "agent"},
            {"id": "report", "name": "Report Generator", "type": "function"},
            {"id": "review", "name": "Review Process", "type": "condition"},
            {"id": "end", "name": "End", "type": "end"}
        ],
        "edges": [
            {"from": "start", "to": "research", "label": "Begin Research"},
            {"from": "research", "to": "analysis", "label": "Research Complete"},
            {"from": "analysis", "to": "report", "label": "Analysis Complete"},
            {"from": "report", "to": "review", "label": "Generate Report"},
            {"from": "review", "to": "end", "label": "Approved"},
            {"from": "review", "to": "analysis", "label": "Needs Revision"}
        ]
    }

def main():
    """Main function for command line usage"""
    if len(sys.argv) < 2:
        print("Usage: python workflow_visualizer.py <format> [workflow_file]")
        print("Formats: mermaid, dot")
        print("If no workflow file is provided, a sample workflow will be used.")
        sys.exit(1)
    
    format_type = sys.argv[1].lower()
    
    if len(sys.argv) > 2:
        # Load workflow from file
        try:
            with open(sys.argv[2], 'r') as f:
                workflow_data = json.load(f)
        except FileNotFoundError:
            print(f"Error: Workflow file '{sys.argv[2]}' not found.")
            sys.exit(1)
        except json.JSONDecodeError:
            print(f"Error: Invalid JSON in workflow file '{sys.argv[2]}'.")
            sys.exit(1)
    else:
        # Use sample workflow
        workflow_data = create_sample_workflow()
    
    if format_type == 'mermaid':
        diagram = generate_mermaid_diagram(workflow_data)
    elif format_type == 'dot':
        diagram = generate_dot_diagram(workflow_data)
    else:
        print(f"Error: Unknown format '{format_type}'. Use 'mermaid' or 'dot'.")
        sys.exit(1)
    
    print(diagram)

if __name__ == "__main__":
    main()
