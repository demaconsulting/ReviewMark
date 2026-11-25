---
name: reviewmark
schema: 2
title: ReviewMark
summary: Built-in help of ReviewMark, which tracks file-review evidence by content fingerprint.
---
{{tool}} groups the files of a project into review sets, fingerprints their content, and checks that a
current review record exists for every set. It writes a review plan and a review report for compliance
documents, and can fail a CI build when a review is missing or out of date.

Every operation is an option. The only command is `{{tool}} help`, which must be the first argument and takes
topic names, such as `{{tool}} help command-line`.
