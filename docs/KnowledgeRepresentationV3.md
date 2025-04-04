# Knowledge Representation Framework (V3 - Functional Approach)

## 1. Introduction

This document outlines the conceptual foundation for Version 3+ of the knowledge representation used within the Reasoning Engine. This version moves towards a more flexible and scalable functional graph structure, drawing inspiration from factor graphs and probabilistic programming concepts. It aims to better support complex probabilistic dependencies (including conditional probabilities), facilitate dynamic graph updates, and provide a clearer path towards future optimizations like GPU acceleration, while maintaining backwards compatibility through versioning.

This framework replaces the previous model (V1/V2) which relied on distinct `SIMONode` and `MISONode` types. V1/V2 node types are no longer supported.

## 2. Core Components

The graph consists of two primary components:

*   **Nodes:** Represent entities, variables, functions, or concepts within the domain.
*   **Edges:** Represent dependencies, relationships, or the flow of information between nodes.

## 3. Nodes (`NodeV3`)

All nodes in V3+ inherit from `NodeBase` and share common properties:

*   `Id`: A unique numerical identifier (long).
*   `Version`: An integer indicating the node's structure version (currently 3).
*   `Content`: A string providing a human-readable description or label for the node.
*   `ExtendedProperties`: A dictionary for storing arbitrary additional metadata.

The key differentiator for `NodeV3` is the `NodeRole` property.

### 3.1. `NodeRole` Enum

This enum defines the fundamental purpose or behavior of a node within the graph:

*   **`Variable`:** Represents a quantity, state, or proposition whose value is uncertain. These nodes are the primary holders of probabilistic information.
*   **`Function`:** Represents a deterministic or stochastic computation or relationship between nodes. These nodes take inputs from parent nodes and produce an output used by child nodes (typically parameters for a child Variable's distribution).
*   **(Future)** `Constant`: Could represent fixed input values.
*   **(Future)** `Evidence`: Could represent observed data clamped onto a `Variable` node.

### 3.2. Role-Specific Properties & Behavior

Based on its `NodeRole`, a `NodeV3` object utilizes specific properties:

*   **If `Role == NodeRole.Variable`:**
    *   `ProbabilityDistribution Distribution`: Holds a `ProbabilityDistribution` object representing the node's current belief state `P(Self)`. This distribution is **not conditional** on parents in its definition; dependencies are handled via Function nodes. It must be non-null.
*   **If `Role == NodeRole.Function`:**
    *   `FunctionType Function`: An enum (`Core/FunctionTypes.cs`) specifying the operation (e.g., `Linear`, `DefinedOp`, `NeuralNet`).
    *   `Dictionary<string, object> FunctionParams`: Stores parameters for the function (e.g., `"Weights"`, `"Bias"` for `Linear`; `"Operation": "Multiply"` for `DefinedOp`).

## 4. Edges (`EdgeV2`)

Edges represent directed dependencies or the flow of information:

*   **Structure:** Defined by `EdgeId` (Guid), `FromNode` (ID), `ToNode` (ID), `Version` (currently 2), `Content` (description), `Weight` (double), and `ExtendedProperties`.
*   **`InputName` Convention:** For edges connecting to a `Function` node (`Variable` -> `Function` or `Function` -> `Function`), the `ExtendedProperties` dictionary *may* contain an `"InputName"` key. This allows `Function` nodes to map specific incoming edges to named arguments if needed (e.g., distinguishing inputs for non-commutative operations). If `"InputName"` is not present, the function might rely on edge order or assume default input roles.
*   **Connectivity Rules & Interpretation:**
    *   `Variable` -> `Function`: Provides the state/value of the Variable as input to the Function.
    *   `Function` -> `Function`: Chains functions; output of the source function is input to the target function.
    *   `Function` -> `Variable`: Represents influence or evidence. The Function's output *influences* the belief state (ProbabilityDistribution) of the target Variable. How this influence is applied depends on the inference algorithm (e.g., updating parameters, applying evidence). **Crucially, the Variable's distribution itself is P(Self), not P(Self | Function Output).**

## 5. Probability Representation

*   **`ProbabilityDistribution` Class:** Represents the belief `P(Self)` for `Variable` nodes. Handles different `DomainType`s. (See separate docs/code).
*   **Dependencies:** Modeled via `Function` nodes. A function `f(Parents)` calculates a result based on parent states. This result then *influences* the belief (`ProbabilityDistribution`) of connected child `Variable` nodes during inference. This differs from directly storing `P(Child | Parents)`.

## 6. Advantages of this Framework

*   **Scalability:** Functional representation avoids exponential CPTs.
*   **Flexibility:** Supports various dependency types via `FunctionType`.
*   **GPU/Linear Algebra Alignment:** Functional dependencies map better to parallel computation.
*   **Clear Semantics:** Separates uncertain `Variable` beliefs from the `Function` nodes that influence them.
*   **Extensibility:** Add new `NodeRole`s or `FunctionType`s.

## 7. Persistence

The file-based JSON persistence layer (`GraphFileManager`) is adapted to serialize/deserialize `NodeV3` and `EdgeV2` (including `EdgeId`). Future work may involve a more scalable backend.

## 8. Future Considerations

*   Implement scalable persistence layer.
*   Add more `FunctionType`s and robust parameter parsing/handling.
*   Implement `Constant` and `Evidence` roles.
*   Develop inference algorithms (belief propagation, etc.) compatible with this structure (how Functions influence Variables).
*   Refine `ProbabilityDistribution` handling (TODOs in `issues.md`).
