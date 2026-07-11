// Views rendered into docs/design/generated/ for the Design document.
// Reopens the ReviewMark package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package ReviewMark {
    view SoftwareStructureView {
        expose ReviewMark;
    }

    view ReviewMarkView {
        expose ReviewMarkSystem;
    }

    view CliView {
        expose CliSubsystem;
    }

    view ConfigurationView {
        expose ConfigurationSubsystem;
    }

    view IndexingView {
        expose IndexingSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }
}
