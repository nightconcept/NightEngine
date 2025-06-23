#import "@preview/rubber-article:0.4.2": *

#show: article.with(
  lang: "en",
  header-display: true,
  header-title: "Project Title: A Subtitle Describing the Core Contribution",
  eq-numbering: "(1.1)",
  eq-chapterwise: true,
  margins: 1.75in,
  cols: none, // Tip: use #colbreak() instead of #pagebreak() to avoid error when useing columns
)

#maketitle(title: "Project Title: A Subtitle Describing the Core Contribution", authors: ("Your Name",), date: datetime
  .today()
  .display("[day]. [month repr:long] [year]"))

#block(width: 100%)[
  *Abstract*

  This paper presents [Project Name], a [brief, high-level description of the artifact, e.g., a bytecode virtual machine for the Lox language]. The project addresses the problem of [state the core problem] by implementing [describe your core technical approach, referencing key algorithms or architectural patterns]. Key features include [mention 2-3 significant features, e.g., a mark-sweep garbage collector, a Pratt parser, etc.]. The system is evaluated against [mention evaluation criteria, e.g., performance benchmarks, feature completeness against a specification]. The results demonstrate [summarize the main finding, e.g., the viability of a register-based VM for dynamic languages on constrained hardware]. This work serves as a practical application of foundational concepts in [mention the relevant CS field, e.g., programming language implementation].
]

#pagebreak()

// ============== Main Body ==============

= Introduction
// Motivate the project. Why is this problem interesting or important?
// Provide context from the relevant field (e.g., Operating Systems, Distributed Systems).
// Clearly state the project's goals and scope.
// End with a "roadmap" for the rest of the paper (e.g., "Section 2 reviews the theoretical background...").

In the field of [Relevant CS Field], a fundamental challenge is [describe the general problem]. Traditional approaches like [mention a standard approach, e.g., tree-walk interpreters] have known limitations in [mention a key limitation, e.g., performance]. This project, [Project Name], explores an alternative approach by [describe your approach].

The primary goals of this project were to:
- Goal 1: [e.g., Implement a complete bytecode compiler and virtual machine].
- Goal 2: [e.g., Gain practical experience with low-level memory management in C].
- Goal 3: [e.g., Achieve a measurable performance improvement over a prior implementation].

This paper is structured as follows. Section 2 discusses the theoretical foundations. Section 3 details the system's architecture and design. Section 4 covers key implementation challenges. Section 5 presents our performance evaluation. Finally, Section 6 concludes and discusses future work.

= Theoretical Foundations & Related Work
// Connect your project to the core curriculum.
// Cite the primary textbooks (e.g., Kleppmann, Arpaci-Dusseau).
// Discuss the core algorithms, data structures, or design patterns you used. For Raft-FS, you would dedicate significant space here to the Raft consensus algorithm. For NightOS, you'd discuss kernel design principles.
// This section demonstrates that you didn't just build something, you understood the theory behind it.

// The design of [Project Name] is heavily influenced by the principles outlined in [Book Title] by [Author] @[CitationKey]. Specifically, the implementation of the [Specific Component] is based on the [Algorithm or Concept Name]...

// Example for Raft-FS:
The Raft consensus algorithm, as described by Ongaro and Ousterhout, provides a foundation for building fault-tolerant distributed systems. It decomposes consensus into three subproblems: leader election, log replication, and safety. Our implementation, Raft-FS, models these states directly...

= System Architecture & Design
// This is the "what" and "why" of your design.
// Use diagrams! Typst can integrate images or even draw simple diagrams.
// Describe the high-level components and how they interact.
// Justify your major design decisions and discuss the trade-offs. Why C# and SDL3 for NightEngine? Why Go for Raft-FS? Why a register-based vs. stack-based VM for Lox?

The system is composed of several major components as illustrated in @fig:architecture.

#figure(
  // Use a placeholder or an actual image of your architecture diagram.
  rect(width: 80%, height: 40%, stroke: black)[High-Level Architecture],
  caption: [A diagram of the major components of Project Name.],
) <fig:architecture>

The choice of [Language/Framework] was motivated by [reason 1, reason 2...]. An alternative, [Alternative Choice], was considered but ultimately rejected due to [explain the trade-off].

== Component A: The Parser
// Use subsections for major components.
The parser is a top-down recursive descent parser, specifically a Pratt parser, which handles infix expressions with varying precedence...

== Component B: The Virtual Machine
The VM is register-based and features a garbage collector based on the mark-sweep algorithm discussed in [Citation]...

= Implementation Challenges & Solutions
// Get specific. Describe 1-3 of the most difficult or interesting problems you solved.
// This is where you show your problem-solving skills.
// Examples: wrestling with manual memory management in C for the Lox VM, implementing the Raft leader election logic, debugging a race condition in a concurrent system.
// Show code snippets to illustrate your points.

One of the most significant challenges was the implementation of the garbage collector. Initial attempts led to memory leaks, as seen in the following simplified example:

#figure(
  raw(
    lang: "c",
    "// A snippet of problematic C code.
void someFunction() {
  Object* obj = newObject(TYPE_STRING);
  // ... forgot to anchor the object before a potential GC run
  collectGarbage(); // Oops! obj might be collected.
}
",
  ),
  caption: [Example of an object losing its root before garbage collection.],
)

The solution involved implementing a system of handles to explicitly anchor objects to the stack...

= Evaluation & Results
// Be a scientist. How do you know if your project was successful?
// Define your metrics for success. (e.g., performance, feature completeness, correctness).
// Present data: benchmarks, graphs of performance, etc.
// Analyze the results. What do they mean? Were they expected?

To evaluate the performance of our bytecode VM, we compared its execution time on a recursive Fibonacci function against the jlox tree-walk interpreter. The benchmark was run on a [Your Machine Specs].

#figure(
  // Placeholder for a results table or graph
  table(
    columns: (1fr, 1fr, 1fr),
    align: (center, center, center),
    [*Input N*], [*jlox (ms)*], [*clox (ms)*],
    [10], [50], [2],
    [20], [4800], [45],
  ),
  caption: [Execution time for fib(N) in milliseconds.],
) <fig:results>

The results in @fig:results show a performance improvement of approximately two orders of magnitude, which is consistent with the expected overhead of a tree-walk interpreter versus a bytecode VM.

= Conclusion & Future Work
// Summarize the project and its key contributions.
// Reiterate what you learned and how it connects to the curriculum's goals.
// Be honest about the limitations of your project. No project is perfect.
// Suggest concrete next steps. What would you do next if you had more time? (e.g., "add a JIT compiler," "implement log compaction in Raft-FS," "support more complex shader programs in NightEngine").

In conclusion, this project successfully demonstrated the implementation of a [artifact] based on principles from [Book/Field]. The primary goals of [state goals] were met, and the resulting system provides a tangible platform for understanding [core concept].

Future work could extend this project in several directions. A just-in-time (JIT) compiler could be added to the VM to further improve performance. Additionally, the type system of the Boba language could be extended to support generics.

= References
// Use a consistent citation style.
// #bibliography("references.bib")
